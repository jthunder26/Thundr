using Thunder.Controllers;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
namespace Thunder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                        {
                        var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("ThundrVault"));
                        config.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
                        })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
     
    }
}
