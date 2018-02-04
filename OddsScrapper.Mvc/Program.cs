using CefSharp;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OddsScrapper.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Cef.Initialize(new CefSettings());

                BuildWebHost(args).Run();
            }
            finally
            {
                // Clean up Chromium objects.  You need to call this in your application otherwise
                // you will get a crash when closing.
                Cef.Shutdown();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
