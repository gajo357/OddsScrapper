using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class ArchiveOddsScrapper
    {
        private HtmlReader WebReader { get; } = new HtmlReader();

        public void Scrape(string baseWebsite, string sport)
        {
            foreach (var league in ReadLeagues($"{baseWebsite}/{sport}/results/", sport))
            {
                var fileName = $"results_{sport}_{league.Country}_{(league.IsFirst ? "first" : "others")}.csv";

                Console.WriteLine(fileName);
                using (var fileStream = File.AppendText(fileName))
                {
                    foreach(var seasonLink in ReadSeasons($"{baseWebsite}{league.Link}"))
                    {
                        Console.WriteLine(seasonLink);
                        foreach (var resultsPage in ReadSeasonPages($"{baseWebsite}{seasonLink}"))
                        {
                            foreach (var resultLine in ReadResultsFromPage(resultsPage))
                            {
                                if (string.IsNullOrEmpty(resultLine))
                                    continue;

                                fileStream.WriteLine(resultLine);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<string> ReadResultsFromPage(HtmlDocument resultsPage)
        {
            var resultsDiv = FindResultsDiv(resultsPage);
            if (resultsDiv == null)
                return new string[] { };

            return FindAndReadResultsTable(resultsDiv);
        }

        private IEnumerable<HtmlDocument> ReadSeasonPages(string link)
        {
            var seasonHtml = WebReader.GetHtmlFromWebpage(link, TournamentTableDivLoaded);

            // first page of the season
            var divTable = FindResultsDiv(seasonHtml);
            if (divTable != null)
                yield return seasonHtml;

            var pagination = divTable.Element(HtmlTagNames.Div);
            if (pagination == null)
                yield break;

            foreach (var resultPage in pagination.ChildNodes)
            {
                if (resultPage.Name == HtmlTagNames.A &&
                    resultPage.FirstChild.GetAttributeValue(HtmlAttributes.Class, null) != "arrow")
                {
                    var pagePage = resultPage.Attributes[HtmlAttributes.Href].Value;
                    var pageResult = WebReader.GetHtmlFromWebpage(pagePage, TournamentTableDivLoaded);
                    yield return pageResult;
                }
            }
        }

        private IEnumerable<string> ReadSeasons(string link)
        {
            var nextYear = "2018";

            var html = WebReader.GetHtmlFromWebpage(link);

            var mainDiv = html.DocumentNode.Descendants(HtmlTagNames.Div).First(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "main-menu2 main-menu-gray");
            var ul = mainDiv.Element(HtmlTagNames.Ul);
            foreach (var a in ul.Descendants(HtmlTagNames.A))
            {
                var seasonResultsLink = a.Attributes[HtmlAttributes.Href].Value;
                var season = a.InnerText;
                if (seasonResultsLink.Contains(nextYear) ||
                    season.Contains(nextYear))
                    continue;

                yield return seasonResultsLink;
            }
        }

        private const string LeaguesTableClassAttribute = "table-main sport";
        private IEnumerable<League> ReadLeagues(string link, string sport)
        {
            var results = new List<League>();

            var page = WebReader.GetHtmlFromWebpage(link);
            if (page == null)
                return results;

            var table = page.DocumentNode.Descendants(HtmlTagNames.Table).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == LeaguesTableClassAttribute);

            IDictionary<string, int> leagueCountryCount = new Dictionary<string, int>();
            foreach (var tr in table.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                // exactly 2 tds in a row
                if (tr.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).Count() != 2)
                    continue;

                foreach(var td in tr.ChildNodes)
                {
                    if (string.IsNullOrEmpty(td.InnerText))
                        continue;

                    var league = new League();
                    league.Link = td.Element(HtmlTagNames.A).Attributes[HtmlAttributes.Href].Value;
                    var noSportLink = league.Link.Replace($"/{sport}/", string.Empty);
                    var country = noSportLink.Remove(noSportLink.IndexOf("/"));
                    league.Country = country;
                    if(leagueCountryCount.ContainsKey(country))
                    {
                        league.IsFirst = false;
                        leagueCountryCount[country]++;
                    }
                    else
                    {
                        league.IsFirst = true;
                        leagueCountryCount.Add(country, 1);
                    }

                    results.Add(league);
                }
            }

            return results;
        }

        private HtmlNode FindResultsDiv(HtmlDocument document)
        {
            return document.DocumentNode.Descendants(HtmlTagNames.Div).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Id, null) == "tournamentTable");
        }

        private IEnumerable<string> FindAndReadResultsTable(HtmlNode divNode)
        {
            var resultsTable = divNode.Element(HtmlTagNames.Table);
            if (resultsTable == null)
                yield break;

            foreach (var tr in resultsTable.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                var attribute = tr.GetAttributeValue(HtmlAttributes.Class, null);
                if (string.IsNullOrEmpty(attribute) ||
                    !attribute.Contains("deactivate"))
                    continue;

                var odds = tr.Elements(HtmlTagNames.Td).Where(s => s.Attributes[HtmlAttributes.Class].Value.Contains("odds-nowrp")).ToArray();
                // has a winner?
                if (!odds.Any(s => s.Attributes[HtmlAttributes.Class].Value.Contains("result-ok")))
                    continue;

                var goodOdd = odds.FirstOrDefault(s => GetOddFromTdNode(s) <= 1.5);
                if (goodOdd == null)
                    continue;

                var odd = GetOddFromTdNode(goodOdd);
                var isWinning = goodOdd.Attributes[HtmlAttributes.Class].Value.Contains("result-ok") ? 1 : 0;

                yield return $"{odd},{isWinning}";
            }
        }

        public static double GetOddFromTdNode(HtmlNode tdNode)
        {
            double odd;
            if (double.TryParse(tdNode.FirstChild.InnerText, out odd))
                return odd;

            return double.NaN;
        }

        private static bool LeaguesTableLoaded(object o)
        {
            var webBrowser = (System.Windows.Forms.WebBrowser)o;

            // WAIT until the dynamic text is set
            foreach (System.Windows.Forms.HtmlElement table in webBrowser.Document.GetElementsByTagName(HtmlTagNames.Table))
            {
                if (table.GetAttribute(HtmlAttributes.ClassName) == LeaguesTableClassAttribute)
                {
                    return !string.IsNullOrEmpty(table.InnerHtml);
                }
            }

            return false;
        }

        private static bool TournamentTableDivLoaded(object o)
        {
            var webBrowser = (System.Windows.Forms.WebBrowser)o;

            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(webBrowser.Document.GetElementById("tournamentTable").InnerText);
        }

        private class League
        {
            public bool IsFirst;
            public string Country;

            public string Link;
        }
    }
}
