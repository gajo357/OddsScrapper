using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace OddsScrapper
{
    public class HtmlReader
    {
        /// <summary>Gets or sets the web browser timeout.</summary>
        public TimeSpan BrowserTimeout { get; } = TimeSpan.FromSeconds(30);
        /// <summary>Gets or sets the web browser delay.</summary>
        public TimeSpan BrowserDelay { get; } = TimeSpan.FromMilliseconds(100);

        //The cookies will be here.
        private CookieContainer Cookies { get; } = new CookieContainer();

        /// <summary>
        /// A browser to use
        /// </summary>
        private System.Windows.Forms.WebBrowser WebBrowser { get; }
        Action DoEventsMethod = System.Windows.Forms.Application.DoEvents;
        
        public HtmlReader()
        {
            WebBrowser = new System.Windows.Forms.WebBrowser() { ScriptErrorsSuppressed = true };
        }

        public HtmlDocument GetHtmlFromWebpage(string webpage, Func<System.Windows.Forms.WebBrowser, bool> isBrowserScriptCompleted = null)
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
        private HtmlDocument LoadFromBrowser(string url, Func<System.Windows.Forms.WebBrowser, bool> isBrowserScriptCompleted)
        {
            var uri = new Uri(url);

            var webBrowser = WebBrowser;

            webBrowser.Navigate(uri, false);

            Stopwatch clock = new Stopwatch();
            clock.Start();

            // WAIT until the document is completed
            while (webBrowser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete || webBrowser.IsBusy)
            {
                if(CheckTimeoutAndToEvents(clock))
                    return null;
            }
                        
            // LOOP until the user say script are completed
            while (!isBrowserScriptCompleted(webBrowser))
            {
                if(CheckTimeoutAndToEvents(clock))
                    return null;
            }            

            var documentText = WebBrowserOuterHtml(webBrowser);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(documentText);            

            return doc;
        }

        private bool CheckTimeoutAndToEvents(Stopwatch clock)
        {
            // ENSURE we didn't reach the timeout
            if (BrowserTimeout.TotalMilliseconds != 0 && clock.ElapsedMilliseconds > BrowserTimeout.TotalMilliseconds)
            {
                return true;
            }

            DoEventsMethod.Invoke();
            Thread.Sleep(BrowserDelay);

            return false;
        }

        internal string WebBrowserOuterHtml(System.Windows.Forms.WebBrowser webBrowser)
        {
            var document = webBrowser.Document;

            var getElementsByTagName = document.GetElementsByTagName("HTML");

            var indexerProperty = getElementsByTagName.GetType().GetProperty("Item", new Type[] { typeof(int) });
            var firstElement = getElementsByTagName[0];

            var outerHtml = firstElement.OuterHtml;

            return outerHtml;
        }

        private HtmlDocument Load(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            //Set more parameters here...
            //...

            //This is the important part.
            request.CookieContainer = Cookies;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //When you get the response from the website, the cookies will be stored
                //automatically in "_cookies".

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    return doc;
                }
            }
            catch
            {
                return null;
            }            
        }
    }
}
