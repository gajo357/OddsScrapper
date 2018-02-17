using CefSharp;

namespace OddsScrapper.WebsiteScrapping
{
    public class ScrapperInitializer
    {
        public static void Initialize()
        {
            Cef.Initialize(new CefSettings());
        }

        public static void CleanUp()
        {
            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }
    }
}
