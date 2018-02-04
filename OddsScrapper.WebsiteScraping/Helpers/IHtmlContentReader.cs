using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Helpers
{
    public interface IHtmlContentReader
    {
        Task<HtmlDocument> GetHtmlFromWebpageAsync(string webpage, Func<HtmlDocument, bool> isBrowserScriptCompleted = null);
    }
}
