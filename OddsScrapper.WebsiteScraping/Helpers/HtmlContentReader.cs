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

        public async Task<HtmlDocument> GetHtmlFromWebpageAsync(string webpage)
        {
            var htmlDoc = await LoadAsync(webpage);

            return htmlDoc;
        }

        private async Task<HtmlDocument> LoadAsync(string url)
        {
            var loaded = await LoadPageAsync(WebBrowser, url);
            return loaded ? await GetHtmlDocumentAsync(WebBrowser.GetBrowser()) : null;
        }

        private static Task<bool> LoadPageAsync(ChromiumWebBrowser browser, string address)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
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

        private static async Task<HtmlDocument> GetHtmlDocumentAsync(IBrowser webBrowser)
        {
            try
            {
                var html = await webBrowser.FocusedFrame.GetSourceAsync();
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
