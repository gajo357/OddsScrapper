using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OddsScrapper.WebsiteScrapping;

namespace OddsScrapper.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                ScrapperInitializer.Initialize();

                BuildWebHost(args).Run();
            }
            finally
            {
                ScrapperInitializer.CleanUp();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
