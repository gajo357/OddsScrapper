using HtmlAgilityPack;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Helpers
{
    public interface IHtmlContentReader
    {
        Task<HtmlDocument> GetHtmlFromWebpageAsync(string webpage);
    }
}
