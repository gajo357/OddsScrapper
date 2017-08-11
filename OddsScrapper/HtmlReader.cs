using HtmlAgilityPack;
using System;

namespace OddsScrapper
{
    public class HtmlReader
    {
        private HtmlWeb Web { get; }

        public HtmlReader()
        {
            Web = new HtmlWeb();
        }

        static HtmlReader()
        {
            // dummy call to load the System.Windows.Forms
            System.Windows.Forms.WebBrowser browser = new System.Windows.Forms.WebBrowser();
        }

        public HtmlDocument GetHtmlFromWebpage(string webpage, Func<object, bool> isBrowserScriptCompleted = null)
        {
            var htmlDoc = isBrowserScriptCompleted != null ?
                Web.LoadFromBrowser(webpage, isBrowserScriptCompleted) :
                Web.Load(webpage);

            return htmlDoc;
        }
    }
}
