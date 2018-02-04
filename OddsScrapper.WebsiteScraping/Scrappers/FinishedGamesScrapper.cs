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
    public class FinishedGamesScrapper : BaseScrapper, IGamesScrapper
    {
        public FinishedGamesScrapper(IArchiveDataRepository repository, IHtmlContentReader reader)
            : base(repository, reader)
        {
        }

        public async Task<IEnumerable<Game>> ScrapeAsync(string baseWebsite, string[] sports, DateTime date)
        {
            var leaguesPage = await Reader.GetHtmlFromWebpageAsync($"{baseWebsite}/results/#{sports[0]}");

            if (leaguesPage == null)
                return null;

            var allGames = new List<Game>();
            foreach (var sportName in sports)
            {
                var sport = await Repository.GetOrCreateSportAsync(sportName);

                foreach (var leagueLink in await ReadLeaguesForSportAsync(leaguesPage, sport))
                {
                    var seasons = await ReadSeasonsAsync($"{baseWebsite}{leagueLink}");

                    foreach (var seasonLink in seasons)
                    {
                        foreach (var resultsPage in await ReadSeasonPagesAsync($"{baseWebsite}{seasonLink}"))
                        {
                            var games = await GetGamesFromDocumentAsync(resultsPage, sport.Name, true);

                            await Repository.InsertGamesAsync(games);
                        }

                        // it is a good question when to persist the changes
                        await Repository.SaveChangesAsync();
                    }
                }
            }


            return allGames;
        }


        private const string LeaguesTableClassAttribute = "table-main sport";
        private async Task<IEnumerable<string>> ReadLeaguesForSportAsync(HtmlDocument page, Sport sport)
        {
            var results = new List<string>();

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

                    var league = await Repository.GetOrCreateLeagueAsync(sport.Name, countryName, leagueName);
                    if (leagueCountryCount.ContainsKey(countryName))
                    {
                        league.IsFirst = false;
                        leagueCountryCount[countryName]++;
                    }
                    else
                    {
                        league.IsFirst = true;
                        leagueCountryCount.Add(countryName, 1);
                    }

                    results.Add(leagueName);
                }
            }

            return results;
        }

        private async Task<IEnumerable<string>> ReadSeasonsAsync(string link)
        {
            var results = new List<string>();

            var html = await Reader.GetHtmlFromWebpageAsync(link);
            if (html == null)
                return results;

            var mainDiv = html.DocumentNode.Descendants(HtmlTagNames.Div).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "main-menu2 main-menu-gray");
            if (mainDiv == null)
                return results;

            var ul = mainDiv.Element(HtmlTagNames.Ul);
            foreach (var a in ul.Descendants(HtmlTagNames.A))
            {
                var seasonResultsLink = a.Attributes[HtmlAttributes.Href].Value;
                var season = a.InnerText;

                if (season.Contains("/"))
                    season = season.Remove(season.IndexOf("/", StringComparison.Ordinal));

                results.Add(seasonResultsLink);
            }

            return results;
        }

        private async Task<IEnumerable<HtmlDocument>> ReadSeasonPagesAsync(string link)
        {
            var results = new List<HtmlDocument>();

            var seasonHtml = await Reader.GetHtmlFromWebpageAsync(link);

            // first page of the season
            var divTable = FindResultsDiv(seasonHtml);
            if (divTable == null)
                return results;

            results.Add(seasonHtml);

            for (var pageIndex = 2; pageIndex <= 50; pageIndex++)
            {
                var pageLink = $"{link}#/page/{pageIndex}/";
                var pageResult = await Reader.GetHtmlFromWebpageAsync(pageLink);
                if (pageResult == null)
                    break;

                results.Add(pageResult);
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
