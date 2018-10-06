using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System.IO;

namespace OddsScraper.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FSharp.CommonScraping.CanopyExtensions.initialize();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}
