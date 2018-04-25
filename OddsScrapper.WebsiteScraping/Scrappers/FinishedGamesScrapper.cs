using HtmlAgilityPack;
using OddsScrapper.Repository.Models;
using OddsScrapper.Repository.Repository;
using OddsScrapper.WebsiteScraping.Helpers;
using OddsScrapper.WebsiteScrapping.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Scrappers
{
    public class FinishedGamesScrapper : BaseScrapper, IGamesScrapper
    {
        public FinishedGamesScrapper(IDbRepository repository, IHtmlContentReader reader)
            : base(repository, reader)
        {
        }

        public async Task<IEnumerable<Game>> ScrapeAsync(string baseWebsite, string[] sports, DateTime date)
        {
            foreach (var leagueDocument in await ReadLeaguePagesForSports(baseWebsite, sports))
            {
                foreach (var leagueLink in ReadLeagueLinksFromLeaguesPage(leagueDocument).Select(s => PrependBase(baseWebsite, s)))
                {
                    var leaguePage = await Reader.GetHtmlFromWebpageAsync(leagueLink);
                    var league = await GetOrCreateLeagueAsync(leagueLink);

                    foreach (var seasonLink in ReadSeasonLinks(leaguePage).Select(s => PrependBase(baseWebsite, s)))
                    {
                        var seasonStartPage = await Reader.GetHtmlFromWebpageAsync(seasonLink);
                        foreach (var seasonPage in await ReadSeasonPages(seasonLink, seasonStartPage))
                        {
                            var games = await GetGamesFromDocumentAsync(baseWebsite, seasonPage, league.Sport, true, league);
                        }
                    }
                }
            }

            return Enumerable.Empty<Game>();
        }

        private async Task<League> GetOrCreateLeagueAsync(string leagueLink)
        {
            var linkParts = leagueLink.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (linkParts.Length < 4)
                return null;

            var sportName = linkParts[2];
            var countryName = linkParts[3];
            var leagueName = linkParts[4];

            if (string.IsNullOrEmpty(sportName) || string.IsNullOrEmpty(leagueName) || string.IsNullOrEmpty(countryName))
                return null;

            var sportTask = Repository.GetOrCreateSportAsync(sportName);
            var countryTask = Repository.GetOrCreateCountryAsync(countryName);

            var league = await Repository.GetOrCreateLeagueAsync(leagueName, false, await sportTask, await countryTask);

            return league;
        }

        private async Task<IEnumerable<HtmlDocument>> ReadLeaguePagesForSports(string baseWebsite, string[] sports)
        {
            var result = new List<HtmlDocument>();
            foreach (var sportName in sports)
                result.Add(await Reader.GetHtmlFromWebpageAsync($"{baseWebsite}/results/#{sportName}"));

            return result;
        }

        private const string LeaguesTableClassAttribute = "table-main sport";
        private IEnumerable<string> ReadLeagueLinksFromLeaguesPage(HtmlDocument page)
        {
            if (string.IsNullOrEmpty(page.DocumentNode.InnerText))
                yield break;

            var tableBody = page
                .DocumentNode
                .Descendants(HtmlTagNames.Table)
                .FirstOrDefault(s => s.AttributeContains(HtmlAttributes.Class, LeaguesTableClassAttribute))
                .Elements(HtmlTagNames.Tbody)
                .FirstOrDefault();
            if (tableBody == null)
                yield break;

            IDictionary<string, int> leagueCountryCount = new Dictionary<string, int>();
            foreach (var tr in tableBody.ChildNodes)
            {
                // exactly 2 tds in a row
                var tds = tr.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                if (tds.Length != 2)
                    continue;

                foreach (var td in tds)
                {
                    if (string.IsNullOrEmpty(td.InnerText))
                        continue;

                    var leagueLink = td.Element(HtmlTagNames.A).Attributes[HtmlAttributes.Href].Value;
                    if (string.IsNullOrEmpty(leagueLink))
                        continue;

                    yield return leagueLink;
                }
            }
        }

        private async Task<IEnumerable<Task<League>>> ReadLeaguesForSportAsync(HtmlDocument page, Sport sport)
        {
            var results = new List<Task<League>>();

            var table = page.DocumentNode.Descendants(HtmlTagNames.Table).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == LeaguesTableClassAttribute);

            IDictionary<string, int> leagueCountryCount = new Dictionary<string, int>();
            foreach (var tr in table.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                // exactly 2 tds in a row
                var tds = tr.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                if (tds.Length != 2)
                    continue;

                foreach (var td in tds)
                {
                    if (string.IsNullOrEmpty(td.InnerText))
                        continue;

                    var leagueLink = td.Element(HtmlTagNames.A).Attributes[HtmlAttributes.Href].Value;
                    if (string.IsNullOrEmpty(leagueLink))
                        continue;

                    if (!leagueLink.Contains($"/{sport.Name}/"))
                        continue;

                    var linkParts = leagueLink.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    if (linkParts.Length < 4)
                        continue;

                    var countryName = linkParts[1];
                    var leagueName = linkParts[2];

                    if (string.IsNullOrEmpty(leagueName) || string.IsNullOrEmpty(countryName))
                        continue;

                    var country = await Repository.GetOrCreateCountryAsync(countryName);

                    var league = Repository.GetOrCreateLeagueAsync(leagueName, !leagueCountryCount.ContainsKey(countryName), sport, country);
                    results.Add(league);

                    if (leagueCountryCount.ContainsKey(countryName))
                    {
                        leagueCountryCount[countryName]++;
                    }
                    else
                    {
                        leagueCountryCount.Add(countryName, 1);
                    }
                }
            }

            return results;
        }

        private IEnumerable<string> ReadSeasonLinks(HtmlDocument leaguePage)
        {
            var mainDiv = leaguePage.DocumentNode.Descendants(HtmlTagNames.Div).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "main-menu2 main-menu-gray");
            if (mainDiv == null)
                yield break;

            var ul = mainDiv.Element(HtmlTagNames.Ul);
            foreach (var a in ul.Descendants(HtmlTagNames.A))
            {
                var seasonResultsLink = a.Attributes[HtmlAttributes.Href].Value;
                var season = a.InnerText;

                if (season.Contains("/"))
                    season = season.Remove(season.IndexOf("/", StringComparison.Ordinal));

                yield return seasonResultsLink;
            }
        }

        private async Task<IEnumerable<HtmlDocument>> ReadSeasonPages(string link, HtmlDocument seasonHtml)
        {
            var results = new List<HtmlDocument>();

            // first page of the season
            var divTable = FindResultsDiv(seasonHtml);
            if (divTable == null)
                return results;

            results.Add(seasonHtml);

            for (var pageIndex = 2; pageIndex <= 20; pageIndex++)
            {
                var pageLink = $"{link}#/page/{pageIndex}/";
                results.Add(await Reader.GetHtmlFromWebpageAsync(pageLink));
            }

            return results;
        }

        private HtmlNode FindResultsDiv(HtmlDocument document)
        {
            var divs = document?.DocumentNode?.Descendants(HtmlTagNames.Div);

            return divs?.FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Id, null) == "tournamentTable");
        }
    }
}
