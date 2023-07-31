using Microsoft.EntityFrameworkCore;
using Stripe;
using Thunder.Data;
using Thunder.Models;
using Thunder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Core.Extensions;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Thunder
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
           
                var thunderDbSecret = Configuration["thundrdb"];

                var thunderBlobStorageSecret = Configuration["thunderblobstorage"];

                var clientSecretSecret = Configuration["clientSecret"];

                var stripeApiKeySecret = Configuration["StripeApiKey"];

                var stripeWebhookSecret = Configuration["StripeEndpointSecret"];

               
                var aioKey = Configuration["AioKey"];

              
                var shipsterKey = Configuration["ShipsterKey"];

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(thunderDbSecret);
                });

                services.AddSingleton<IStripeClient>(x => new Stripe.StripeClient(stripeApiKeySecret));

                services.AddSingleton<IBackgroundQueueService>(provider =>
                {
                    var blobStorageConnection = Configuration["thunderblobstorage"];
                    var createLabelQueueName = "createlabelqueue";
                    var retrieveStoreQueueName = "retrievestorequeue";
                    return new BackgroundQueueService(blobStorageConnection, createLabelQueueName, retrieveStoreQueueName);
                });

                services.AddHostedService<CreateLabelBackgroundService>();
                services.AddHostedService<RetrieveAndStoreLabelBackgroundService>();

                services.Configure<StripeOptions>(options =>
                {
                    options.ApiKey = stripeApiKeySecret;
                    options.WebhookSecret = stripeWebhookSecret;
                });
                services.AddTransient<IMailService, MailService>();
                services.AddScoped<IBlobService>(provider =>
                {
                    var connectionString = thunderBlobStorageSecret;
                    var containerName = "pdfcontainer";
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var mailService = provider.GetRequiredService<IMailService>(); // Get the IMailService instance
                    return new BlobService(blobServiceClient, containerName, mailService); // Pass the IMailService instance to BlobService
                });


                services.AddAuthentication()
                    .AddMicrosoftAccount(options =>
                    {
                        options.ClientId = "5bad5faa-9e81-47d3-828e-48a2e68f46af";
                        options.ClientSecret = clientSecretSecret;
                        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                    });

                services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.User.RequireUniqueEmail = true;
                })
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();


            services.AddScoped<IThunderService>(provider =>
            {
                var db = provider.GetRequiredService<ApplicationDbContext>();
                var blobService = provider.GetRequiredService<IBlobService>();
                var userService = provider.GetRequiredService<IUserService>();
                var logger = provider.GetRequiredService<ILogger<ThunderService>>();

                var aioKey = Configuration["AioKey"];
                var shipsterKey = Configuration["ShipsterKey"];

                return new ThunderService(db, aioKey, shipsterKey, blobService, userService, logger);
            });

            services.AddScoped<IUpsRateService, UpsRateService>();
                services.AddScoped<IUserService, UserService>();
               
                services.AddHttpContextAccessor();

                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });

                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(
                        policy =>
                        {
                            policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                });
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => false; // cookies can be set without user consent
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

                services.AddControllersWithViews();
                services.AddRazorPages();
            
        }

        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            await EnsureRoles(services);
        }

        private async Task EnsureRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try
            {
                if (!await roleManager.RoleExistsAsync("Admin").ConfigureAwait(false))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin")).ConfigureAwait(false);
                    await roleManager.CreateAsync(new IdentityRole("User")).ConfigureAwait(false);

                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine(ex);
            }
        }
    }
}
