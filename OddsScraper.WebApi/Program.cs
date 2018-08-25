using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OddsScraper.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FSharp.Scraping.CanopyExtensions.initialize();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
