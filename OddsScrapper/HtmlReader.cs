using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OddsScrapper
{
    public class HtmlReader
    {
        /// <summary>
        /// A browser to use
        /// </summary>
        private ChromiumWebBrowser WebBrowser { get; }

        public HtmlReader()
        {
            WebBrowser = new ChromiumWebBrowser();
        }

        public HtmlDocument GetHtmlFromWebpage(string webpage, Func<HtmlDocument, bool> isBrowserScriptCompleted = null)
        {
            var htmlDoc = isBrowserScriptCompleted != null ?
                LoadFromBrowser(webpage, isBrowserScriptCompleted) :
                Load(webpage);

            return htmlDoc;
        }

        /// <summary>Loads HTML using a WebBrowser and Application.DoEvents.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="url">The requested URL, such as "http://html-agility-pack.net/".</param>
        /// <param name="isBrowserScriptCompleted">Check if the browser script has all been run and completed.</param>
        /// <returns>A new HTML document.</returns>
        private HtmlDocument LoadFromBrowser(string url, Func<HtmlDocument, bool> isBrowserScriptCompleted)
        {
            var webBrowser = WebBrowser;

            LoadPageAsync(webBrowser, url).Wait();

            var document = GetHtmlDocument(webBrowser);
            if (!isBrowserScriptCompleted(document))
            {
                return null;
            }

            return document;
        }

        private HtmlDocument Load(string url)
        {
            LoadPageAsync(WebBrowser, url).Wait();
            return GetHtmlDocument(WebBrowser);
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

            Stopwatch clock = new Stopwatch();
            clock.Start();


            return tcs.Task;
        }

        private static HtmlDocument GetHtmlDocument(ChromiumWebBrowser webBrowser)
        {
            try
            {
                var mainFrame = webBrowser.GetMainFrame();
                var html = mainFrame.GetSourceAsync().Result;
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
