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

        public void GetRecentResults(string baseWebsite, string[] sports)
        {
            // page with results of all sports
            var leaguesPage = ReadLeaguesPage(baseWebsite, sports[0]);
            if (leaguesPage == null)
                return;

            var filename = Path.Combine(HelperMethods.GetArchiveFolderPath(), $"recentdata.csv");
            using (var fileStream = File.AppendText(filename))
            {
                fileStream.WriteLine("Sport,Country,League,Season,Participants,Best Odd,Your Bet,Winning Bet");

                foreach (var sport in sports)
                {
                    foreach (var league in ReadLeaguesForSport(leaguesPage, sport))
                    {
                        Console.WriteLine(league.Name);

                        var resultsPage = WebReader.GetHtmlFromWebpage($"{baseWebsite}{league.Link}", TournamentTableDivLoaded);
                        foreach (var resultLine in ReadResultsFromPage(resultsPage))
                        {
                            if (string.IsNullOrEmpty(resultLine))
                                continue;

                            fileStream.WriteLine($"{sport},{league.Country},{league.Name},2018,{resultLine}");
                        }
                    }
                }
            }
        }
        
        public void Scrape(string baseWebsite, string[] sports)
        {
            // page with results of all sports
            var leaguesPage = ReadLeaguesPage(baseWebsite, sports[0]);
            if (leaguesPage == null)
                return;

            foreach (var sport in sports)
            {
                var leagueFileName = Path.Combine(HelperMethods.GetArchiveFolderPath(), $"leagues_{sport}.txt");
                File.AppendAllLines(leagueFileName, new[] { "Sport,Country,LeagueName,IsFirst" });
                foreach (var league in ReadLeaguesForSport(leaguesPage, sport))
                {
                    var fileName = HelperMethods.MakeValidFileName($"{league.Sport}_{league.Country}_{league.Name}.csv");

                    Console.WriteLine(fileName);
                    File.AppendAllLines(leagueFileName, new[] { league.ToString() });
                    using (var fileStream = File.AppendText(Path.Combine(HelperMethods.GetArchiveFolderPath(), fileName)))
                    {
                        fileStream.WriteLine("Season,Participants,Best Odd,Your Bet,Winning Bet");

                        var seasons = ReadSeasons($"{baseWebsite}{league.Link}");
                        if (seasons == null)
                            continue;
                        foreach (var seasonInfo in seasons)
                        {
                            var seasonLink = seasonInfo.Item2;
                            var season = seasonInfo.Item1;

                            Console.WriteLine(seasonLink);
                            foreach (var resultsPage in ReadSeasonPages($"{baseWebsite}{seasonLink}"))
                            {
                                foreach (var resultLine in ReadResultsFromPage(resultsPage))
                                {
                                    if (string.IsNullOrEmpty(resultLine))
                                        continue;

                                    fileStream.WriteLine($"{season},{resultLine}");
                                }
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
            if (divTable == null)
                yield break;

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

        private IEnumerable<Tuple<string, string>> ReadSeasons(string link)
        {
            var nextYear = "2018";
            var lastYear = new[] { "1999", "1998", "1997", "1996" };

            var html = WebReader.GetHtmlFromWebpage(link);
            if (html == null)
                yield break;

            var mainDiv = html.DocumentNode.Descendants(HtmlTagNames.Div).First(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "main-menu2 main-menu-gray");
            var ul = mainDiv.Element(HtmlTagNames.Ul);
            foreach (var a in ul.Descendants(HtmlTagNames.A))
            {
                var seasonResultsLink = a.Attributes[HtmlAttributes.Href].Value;
                var season = a.InnerText;

                // reached the last year
                if (lastYear.Any(seasonResultsLink.Contains) ||
                    lastYear.Any(season.Contains))
                    break;
                // skip the current year
                if (seasonResultsLink.Contains(nextYear) ||
                    season.Contains(nextYear))
                    continue;

                if (season.Contains("/"))
                    season = season.Remove(season.IndexOf("/", StringComparison.Ordinal));

                yield return new Tuple<string, string>(season, seasonResultsLink);
            }
        }

        private const string LeaguesTableClassAttribute = "table-main sport";
        private HtmlDocument ReadLeaguesPage(string baseWebsite, string sport)
        {
            // page with results of all sports
            var leaguesPage = WebReader.GetHtmlFromWebpage($"{baseWebsite}/results/#{sport}", LeaguesTableLoaded);
            return leaguesPage;
        }

        private IEnumerable<LeagueInfo> ReadLeaguesForSport(HtmlDocument page, string sport)
        {
            var results = new List<LeagueInfo>();

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

                    var league = new LeagueInfo();
                    league.Link = td.Element(HtmlTagNames.A).Attributes[HtmlAttributes.Href].Value;
                    if (string.IsNullOrEmpty(league.Link))
                        continue;

                    if (!league.Link.Contains($"/{sport}/"))
                        continue;

                    var linkParts = league.Link.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    if (linkParts.Length < 4)
                        continue;

                    var country = linkParts[1];
                    var leagueName = linkParts[2];

                    if (string.IsNullOrEmpty(leagueName) ||
                        string.IsNullOrEmpty(country))
                        continue;

                    league.Country = country;
                    league.Name = leagueName;
                    if (leagueCountryCount.ContainsKey(country))
                    {
                        league.IsFirst = false;
                        leagueCountryCount[country]++;
                    }
                    else
                    {
                        league.IsFirst = true;
                        leagueCountryCount.Add(country, 1);
                    }

                    var leagueNameParts = leagueName.Split('-');
                    league.Sport = sport;

                    results.Add(league);
                }
            }
            
            return results;
        }

        private HtmlNode FindResultsDiv(HtmlDocument document)
        {
            var divs = document?.DocumentNode?.Descendants(HtmlTagNames.Div);

            return divs?.FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Id, null) == "tournamentTable");
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

                var participants = string.Empty;
                var participantElement = tr.Elements(HtmlTagNames.Td).FirstOrDefault(s => s.Attributes[HtmlAttributes.Class].Value.Contains("name table-participant"));
                if (participantElement != null)
                    participants = participantElement.InnerText;

                var odds = tr.Elements(HtmlTagNames.Td).Where(s => s.Attributes[HtmlAttributes.Class].Value.Contains("odds-nowrp")).ToArray();
                // has a winner?
                if (!odds.Any(s => s.Attributes[HtmlAttributes.Class].Value.Contains("result-ok")))
                    continue;

                int winIndex = -1;
                int oddIndex = -1;
                double odd = 0;
                for (var i = 0; i < odds.Length; i++)
                {
                    var nodeOdd = GetOddFromTdNode(odds[i]);
                    if (nodeOdd <= 1.5)
                    {
                        odd = nodeOdd;
                        oddIndex = i;
                    }

                    if (odds[i].Attributes[HtmlAttributes.Class].Value.Contains("result-ok"))
                        winIndex = i;
                }

                if (winIndex < 0 ||
                    oddIndex < 0)
                    continue;
                
                // 1 is home win, 2 is away win, 0 is draw
                int winCombo = HelperMethods.GetBetComboFromIndex(odds.Length, winIndex);
                int betCombo = HelperMethods.GetBetComboFromIndex(odds.Length, oddIndex);

                yield return $"{participants},{odd},{betCombo},{winCombo}";
            }
        }

        public static double GetOddFromTdNode(HtmlNode tdNode)
        {
            double odd;
            if (double.TryParse(tdNode.FirstChild.InnerText, out odd))
                return odd;

            return double.NaN;
        }

        private static bool LeaguesTableLoaded(System.Windows.Forms.WebBrowser webBrowser)
        {
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

        private static bool TournamentTableDivLoaded(System.Windows.Forms.WebBrowser webBrowser)
        {
            var table = webBrowser.Document.GetElementById("tournamentTable");
            if (table == null)
                return false;

            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(table.InnerText);
        }        
    }
}
