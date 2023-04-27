using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Thunder.Data;
using Thunder.Models;
using Thunder.Services;

namespace Thunder
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
            }) 
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddServerSideBlazor();
            services.AddControllersWithViews();
            //services.AddHttpClient();
            services.AddRazorPages();
            services.Configure<StripeOptions>(Configuration.GetSection("Stripe"));
            services.AddScoped<IThunderService, ThunderService>();
            services.AddScoped<IUpsRateService, UpsRateService>();
            services.AddScoped<IStripeService, StripeService>();
            services.AddTransient<IMailService, MailService>(); //transient bc we want a new object with each request. 
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

            //CORS should be configured with a strict policy to allow only the required origins, headers,
            //and methods. Allowing any origin, header, or method could expose your application to security risks.
            //Consider updating the CORS configuration to something like this:
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("MyPolicy",
            //        builder =>
            //        {
            //            builder.WithOrigins("https://example.com")
            //                   .AllowAnyHeader()
            //                   .AllowAnyMethod();
            //        });
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //StripeConfiguration.ApiKey = "sk_test_51MxFCnDHpayIZlcAaiJXTw7ln9gD8sPbzmNtN9bBIwFmhrOMhGcoLlWHkbrE8EHUvYDmsoU7e8iCY0Jh0SWRFH8N00sbrQOelZ";

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseAuthentication();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
            });
        }
    }
}