using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Helpers
{
    public class HtmlContentReader : IHtmlContentReader
    {
        /// <summary>
        /// A browser to use
        /// </summary>
        private ChromiumWebBrowser WebBrowser { get; }

        public HtmlContentReader()
        {
            WebBrowser = new ChromiumWebBrowser();
        }

        public Task<HtmlDocument> GetHtmlFromWebpageAsync(string webpage, Func<HtmlDocument, bool> isBrowserScriptCompleted = null)
        {
            var htmlDoc = isBrowserScriptCompleted != null ?
                LoadFromBrowserAsync(webpage, isBrowserScriptCompleted) :
                LoadAsync(webpage);

            return htmlDoc;
        }

        /// <summary>Loads HTML using a WebBrowser and Application.DoEvents.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="url">The requested URL, such as "http://html-agility-pack.net/".</param>
        /// <param name="isBrowserScriptCompleted">Check if the browser script has all been run and completed.</param>
        /// <returns>A new HTML document.</returns>
        private async Task<HtmlDocument> LoadFromBrowserAsync(string url, Func<HtmlDocument, bool> isBrowserScriptCompleted)
        {
            var webBrowser = WebBrowser;

            await LoadPageAsync(webBrowser, url);

            var document = await GetHtmlDocumentAsync(webBrowser);
            if (!isBrowserScriptCompleted(document))
            {
                return null;
            }

            return document;
        }

        private async Task<HtmlDocument> LoadAsync(string url)
        {
            await LoadPageAsync(WebBrowser, url);
            return await GetHtmlDocumentAsync(WebBrowser);
        }

        private static Task LoadPageAsync(ChromiumWebBrowser browser, string address = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler += (sender, args) =>
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            browser.Load(address);

            return tcs.Task;
        }

        private static async Task<HtmlDocument> GetHtmlDocumentAsync(ChromiumWebBrowser webBrowser)
        {
            try
            {
                var mainFrame = webBrowser.GetMainFrame();
                var html = await mainFrame.GetSourceAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
            catch
            {
                return null;
            }
        }
    }
}
