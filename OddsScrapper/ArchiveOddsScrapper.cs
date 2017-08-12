using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OddsScrapper
{
    public class ArchiveOddsScrapper
    {
        private HtmlReader WebReader { get; } = new HtmlReader();

        public void ScrapeLeagueInfo(string baseWebsite, string sport)
        {
            // page with results of all sports
            var leaguesPage = ReadLeaguesPage(baseWebsite, sport);
            if (leaguesPage == null)
                return;

            foreach (var league in ReadLeaguesForSport(leaguesPage, sport))
            {
                File.AppendAllLines($"leagues_{sport}.txt", new[] { league.ToString() });
            }
        }

        public void Scrape(string baseWebsite, string[] sports)
        {
            // page with results of all sports
            var leaguesPage = ReadLeaguesPage(baseWebsite, sports[0]);
            if (leaguesPage == null)
                return;

            Parallel.ForEach(sports, (sport) =>
            {
                foreach (var league in ReadLeaguesForSport(leaguesPage, sport))
                {
                    var fileName = MakeValidFileName($"{league.Sport}_{league.Country}_{league.Name}.csv");

                    Console.WriteLine(fileName);
                    File.AppendAllLines($"leagues_{sport}.txt", new[] { league.ToString() });
                    using (var fileStream = File.AppendText(fileName))
                    {
                        var seasons = ReadSeasons($"{baseWebsite}{league.Link}");
                        if (seasons == null)
                            continue;
                        foreach (var seasonLink in seasons)
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
            });            
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

        private IEnumerable<string> ReadSeasons(string link)
        {
            var nextYear = "2018";
            var lastYear = new[] { "2009", "2008", "2007", "2006" };

            var html = WebReader.GetHtmlFromWebpage(link);
            if (html == null)
                yield break;

            var mainDiv = html.DocumentNode.Descendants(HtmlTagNames.Div).First(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "main-menu2 main-menu-gray");
            var ul = mainDiv.Element(HtmlTagNames.Ul);
            foreach (var a in ul.Descendants(HtmlTagNames.A))
            {
                var seasonResultsLink = a.Attributes[HtmlAttributes.Href].Value;
                var season = a.InnerText;
                if (lastYear.Any(seasonResultsLink.Contains) ||
                    lastYear.Any(season.Contains))
                    break;
                if (seasonResultsLink.Contains(nextYear) ||
                    season.Contains(nextYear))
                    continue;

                yield return seasonResultsLink;
            }
        }

        private const string LeaguesTableClassAttribute = "table-main sport";
        private HtmlDocument ReadLeaguesPage(string baseWebsite, string sport)
        {
            // page with results of all sports
            var leaguesPage = WebReader.GetHtmlFromWebpage($"{baseWebsite}/results/#{sport}", LeaguesTableLoaded);
            return leaguesPage;
        }

        private string[] CupNames = new[] { "cup", "copa", "cupen" };
        private string Women = "women";
        private IEnumerable<League> ReadLeaguesForSport(HtmlDocument page, string sport)
        {
            var results = new List<League>();

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

                    var league = new League();
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
                    league.IsCup = CupNames.Any(s => leagueNameParts.Any(x => x.Contains(s)));
                    league.IsWomen = leagueName.Contains(Women);
                    league.Sport = sport;

                    results.Add(league);
                }
            }
            
            return results;
        }

        private HtmlNode FindResultsDiv(HtmlDocument document)
        {
            if (document == null ||
                document.DocumentNode == null)
                return null;

            var divs = document.DocumentNode.Descendants(HtmlTagNames.Div);
            if (divs == null)
                return null;

            return divs.FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Id, null) == "tournamentTable");
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

                var goodOdd = odds.FirstOrDefault(s => GetOddFromTdNode(s) <= 1.5);
                if (goodOdd == null)
                    continue;

                var odd = GetOddFromTdNode(goodOdd);
                var isWinning = goodOdd.Attributes[HtmlAttributes.Class].Value.Contains("result-ok") ? 1 : 0;

                yield return $"{participants},{odd},{isWinning}";
            }
        }

        public static double GetOddFromTdNode(HtmlNode tdNode)
        {
            double odd;
            if (double.TryParse(tdNode.FirstChild.InnerText, out odd))
                return odd;

            return double.NaN;
        }

        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
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

        private class League
        {
            public bool IsFirst;
            public string Country;
            public string Name;

            public string Link;
            internal bool IsCup;
            internal bool IsWomen;
            internal string Sport;

            public override string ToString()
            {
                return $"{Sport},{Country},{MakeValidFileName(Name)},{(IsFirst ? 1 : 0)},{(IsCup ? 1 : 0)},{(IsWomen ? 1 : 0)}";
            }            
        }
    }
}
