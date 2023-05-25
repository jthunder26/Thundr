﻿using Microsoft.EntityFrameworkCore;
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
            var keyVaultEndpoint = Configuration["KeyVault:Endpoint"];
            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                var credential = new DefaultAzureCredential();
                var secretClient = new SecretClient(new Uri(keyVaultEndpoint), credential);

                var thunderDbSecretName = "thundrdb";
                var thunderDbSecret = secretClient.GetSecret(thunderDbSecretName);

                var thunderBlobStorageSecretName = "thunderblobstorage";
                var thunderBlobStorageSecret = secretClient.GetSecret(thunderBlobStorageSecretName);

                var clientSecretSecretName = "clientSecret";
                var clientSecretSecret = secretClient.GetSecret(clientSecretSecretName);

                //UNCOMMENT WHEN GOING LIVE AND COMMENT OUT THE TEST ONE
                //var stripeApiKeySecretName = "StripeApiKey";
                var stripeApiKeySecretName = "StripeTestApiKey";
                var stripeApiKeySecret = secretClient.GetSecret(stripeApiKeySecretName);

                var stripeWebhookSecretName = "StripeEndpointSecret";
                var stripeWebhookSecret = secretClient.GetSecret(stripeWebhookSecretName);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(thunderDbSecret.Value.Value);
                   

                });

                services.AddSingleton<IStripeClient>(x => new Stripe.StripeClient(stripeApiKeySecret.Value.Value));

                services.Configure<StripeOptions>(options =>
                {
                    options.ApiKey = stripeApiKeySecret.Value.Value;
                    options.WebhookSecret = stripeWebhookSecret.Value.Value;
                });

                services.AddScoped<IBlobService>(provider =>
                {
                    var connectionString = thunderBlobStorageSecret.Value.Value;
                    var containerName = "pdfcontainer";
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    return new BlobService(blobServiceClient, containerName);
                });

                services.AddAuthentication()
                    .AddMicrosoftAccount(options =>
                    {
                        options.ClientId = "5bad5faa-9e81-47d3-828e-48a2e68f46af";
                        options.ClientSecret = clientSecretSecret.Value.Value;
                        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                    });

                services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.User.RequireUniqueEmail = true;
                })
                    .AddEntityFrameworkStores<ApplicationDbContext>();

                services.AddSingleton(secretClient);
                services.AddScoped<IThunderService, ThunderService>();
                services.AddScoped<IUpsRateService, UpsRateService>();
                services.AddScoped<IUserService, UserService>();
                services.AddTransient<IMailService, MailService>();
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

                services.AddControllersWithViews();
                services.AddRazorPages();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseAuthentication();
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
        }
    }
}
