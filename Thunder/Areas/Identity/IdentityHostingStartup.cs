[assembly: HostingStartup(typeof(Thunder.Areas.Identity.IdentityHostingStartup))]
namespace Thunder.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}
