using HtmlAgilityPack;
using OddsScrapper.Shared.Models;
using OddsScrapper.Shared.Repository;
using OddsScrapper.WebsiteScraping.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Scrappers
{
    public class UnfinishedGamesScrapper : BaseScrapper, IGamesScrapper
    {
        public UnfinishedGamesScrapper(IArchiveDataRepository repository, IHtmlContentReader reader)
            : base(repository, reader)
        {
        }

        public async Task<IEnumerable<Game>> ScrapeAsync(string baseWebsite, string[] sports, DateTime date)
        {
            var allGames = new List<Game>();

            foreach(var sport in sports)
            {
                var gamesDocument = await GetGamesPlayedOnDateDocumentAsync(baseWebsite, sport, date);
                var games = await GetGamesFromDocumentAsync(gamesDocument, sport, false);

                allGames.AddRange(games);
            }

            await Repository.SaveChangesAsync();

            return allGames;
        }

        private async Task<HtmlDocument> GetGamesPlayedOnDateDocumentAsync(string baseWebsite, string sport, DateTime date)
        {
            var link = $"{baseWebsite}/matches/{sport}/";
            var page = await Reader.GetHtmlFromWebpageAsync(link);
            if (page == null)
                return null;

            var datesSpan = page.DocumentNode.Descendants(HtmlTagNames.Span).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "next-games-date");
            if (datesSpan == null)
                return null;

            var dateAttribute = GetDateAsStringAttribute(date);
            var a = datesSpan.Elements(HtmlTagNames.A).FirstOrDefault(s => s.InnerText.ToUpperInvariant().Contains(dateAttribute));
            if (a == null)
                return null;

            var gamesLink = a.Attributes[HtmlAttributes.Href].Value;
            if (!gamesLink.StartsWith(baseWebsite))
                gamesLink = $"{baseWebsite}{gamesLink}";

            return await Reader.GetHtmlFromWebpageAsync(gamesLink);
        }

        private string GetDateAsStringAttribute(DateTime date)
        {
            string dateAttribute;
            if (date.Date == DateTime.Today)
            {
                dateAttribute = "TODAY";
            }
            else if (date.Date == DateTime.Today.AddDays(1))
            {
                dateAttribute = "TOMORROW";
            }
            else if (date.Date == DateTime.Today.AddDays(-1))
            {
                dateAttribute = "YESTERDAY";
            }
            else
            {
                dateAttribute = DateTime.Today.ToString("dd MMM YYYY").ToUpperInvariant();
            }

            return dateAttribute;
        }
    }
}
