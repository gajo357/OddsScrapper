using HtmlAgilityPack;
using OddsScrapper.DbModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class ArchiveOddsScrapper
    {
        private HtmlReader WebReader { get; } = new HtmlReader();
        private DbRepository DbRepository { get; } = new DbRepository(Path.Combine(HelperMethods.GetSolutionDirectory(), "ArchiveData1.db")); 

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
                            //if (string.IsNullOrEmpty(resultLine))
                            //    continue;

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
                var sportId = DbRepository.GetSportId(sport);

                foreach (var league in ReadLeaguesForSport(leaguesPage, sport))
                {
                    var countryId = DbRepository.GetOrCreateCountryId(league.Country);
                    var leagueId = DbRepository.GetOrCreateLeagueId(sportId, countryId, league);

                    Console.WriteLine(league.Name);

                    var seasons = ReadSeasons(WebReader, $"{baseWebsite}{league.Link}");
                    if (seasons == null)
                        continue;
                    foreach (var seasonInfo in seasons)
                    {
                        var seasonLink = seasonInfo.Item2;
                        int season;
                        if(!int.TryParse(seasonInfo.Item1, out season))
                            season = DateTime.Now.Year;

                        Console.WriteLine(seasonLink);
                        foreach (var resultsPage in ReadSeasonPages(WebReader, $"{baseWebsite}{seasonLink}"))
                        {
                            DbRepository.InsertGames(leagueId, season, ReadResultsFromPage(resultsPage));
                        }
                    }
                }
            }
        }

        private IEnumerable<Game> ReadResultsFromPage(HtmlDocument resultsPage)
        {
            var resultsDiv = FindResultsDiv(resultsPage);
            if (resultsDiv == null)
                return Enumerable.Empty<Game>();

            return FindAndReadResultsTable(resultsDiv);
        }

        private IEnumerable<HtmlDocument> ReadSeasonPages(HtmlReader reader, string link)
        {
            var seasonHtml = reader.GetHtmlFromWebpage(link, TournamentTableDivLoaded);

            // first page of the season
            var divTable = FindResultsDiv(seasonHtml);
            if (divTable == null)
                yield break;

            yield return seasonHtml;

            for (var pageIndex = 2; pageIndex <= 50; pageIndex++)
            {
                var pagePage = $"{link}#/page/{pageIndex}/";
                var pageResult = reader.GetHtmlFromWebpage(pagePage, TournamentTableDivLoaded);
                if (pageResult == null)
                    yield break;

                yield return pageResult;
            }
        }

        private IEnumerable<Tuple<string, string>> ReadSeasons(HtmlReader reader, string link)
        {
            var nextYear = "2018";
            var lastYear = new[] { "1995", "1994", "1993", "1992" };

            var html = reader.GetHtmlFromWebpage(link);
            if (html == null)
                yield break;

            var mainDiv = html.DocumentNode.Descendants(HtmlTagNames.Div).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "main-menu2 main-menu-gray");
            if (mainDiv == null)
                yield break;

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

        private string[] CupNames = new[] { "cup", "copa", "cupen", "coupe", "coppa" };
        private string Women = "women";
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
                    league.Sport = sport;

                    var leagueNameParts = leagueName.Split('-');
                    league.IsCup = CupNames.Any(leagueNameParts.Contains);
                    league.IsWomen = leagueNameParts.Contains(Women);

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

        private IEnumerable<Game> FindAndReadResultsTable(HtmlNode divNode)
        {
            const string dateFromat = "dd MMM yyyy";

            var resultsTable = divNode.Element(HtmlTagNames.Table);
            if (resultsTable == null)
                yield break;

            var date = DateTime.MinValue;
            var isPlayoffs = false;
            foreach (var tr in resultsTable.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                var attribute = tr.GetAttributeValue(HtmlAttributes.Class, null);
                if (string.IsNullOrEmpty(attribute))
                    continue;

                if (attribute.Contains("center nob-border"))
                {
                    // date
                    var dateElement = tr.Element("th").Element(HtmlTagNames.Span);
                    if (dateElement == null)
                        continue;

                    var dateAttribute = dateElement.GetAttributeValue(HtmlAttributes.Class, null);
                    if (string.IsNullOrEmpty(dateAttribute) || !dateAttribute.Contains("datet"))
                        continue;

                    isPlayoffs = tr.InnerText.Contains("Play Offs");


                    var dateString = dateElement.InnerText;
                    if (dateString.Contains(","))
                    {
                        dateString = dateString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        var year = DateTime.Now.Year;
                        dateString = $"{dateString} {year}";
                    }

                    DateTime.TryParseExact($"{dateString}", dateFromat, null, DateTimeStyles.None, out date);

                    continue;
                }

                if (!attribute.Contains("deactivate"))
                    continue;

                var oddTags = tr.Elements(HtmlTagNames.Td).Where(s => s.Attributes[HtmlAttributes.Class].Value.Contains("odds-nowrp")).ToArray();
                // has a winner?
                if (!oddTags.Any(s => s.Attributes[HtmlAttributes.Class].Value.Contains("result-ok")))
                    continue;

                var participantsString = string.Empty;
                var participantElement = tr.Elements(HtmlTagNames.Td).FirstOrDefault(s => s.Attributes[HtmlAttributes.Class].Value.Contains("name table-participant"));
                if (participantElement != null)
                    participantsString = participantElement.InnerText;

                var scoreString = string.Empty;
                var scoreElement = tr.Elements(HtmlTagNames.Td).FirstOrDefault(s => s.Attributes[HtmlAttributes.Class].Value.Contains("table-score"));
                if (scoreElement != null)
                    scoreString = scoreElement.InnerText;

                int winIndex = -1;
                int lowestOddIndex = -1;
                var lowestOdd = double.MaxValue;
                double homeOdd = 0;
                double drawOdd = 0;
                double awayOdd = 0;
                for (var i = 0; i < oddTags.Length; i++)
                {
                    var nodeOdd = HelperMethods.GetOddFromTdNode(oddTags[i]);
                    if (i == 0)
                    {
                        homeOdd = nodeOdd;
                    }
                    else if (i == 1 && oddTags.Length == 3)
                    {
                        drawOdd = nodeOdd;
                    }
                    else
                    {
                        awayOdd = nodeOdd;
                    }

                    if (nodeOdd < lowestOdd)
                    {
                        lowestOdd = nodeOdd;
                        lowestOddIndex = i;
                    }

                    if (oddTags[i].Attributes[HtmlAttributes.Class].Value.Contains("result-ok"))
                    {
                        winIndex = i;
                    }
                }

                // 1 is home win, 2 is away win, 0 is draw
                int winCombo = HelperMethods.GetBetComboFromIndex(oddTags.Length, winIndex);
                int betCombo = HelperMethods.GetBetComboFromIndex(oddTags.Length, lowestOddIndex);

                var participants = participantsString.Replace("&nbsp;", string.Empty).Replace("&amp;", "and").Replace("'", " ").Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (participants.Length < 2)
                    continue;

                var isOvertime = false;
                if (scoreString.Contains("&nbsp;"))
                    //|| scoreString.Contains("OT") 
                    //|| scoreString.Contains("pen.")
                    //|| scoreString.Contains("ET"))
                {
                    isOvertime = true;
                    scoreString = scoreString.Remove(scoreString.IndexOf("&nbsp;"));
                }
                var scores = scoreString.Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (scores.Length < 2)
                    continue;
                int homeScore;
                if (!int.TryParse(scores[0], out homeScore))
                    continue;
                int awayScore;
                if (!int.TryParse(scores[1], out awayScore))
                    continue;

                var game = new Game();
                game.HomeTeam = participants[0];
                game.AwayTeam = participants[1];
                game.HomeOdd = homeOdd;
                game.DrawOdd = drawOdd;
                game.AwayOdd = awayOdd;
                game.Bet = betCombo;
                game.Winner = winCombo;
                game.Date = date;
                game.IsOvertime = isOvertime;
                game.IsPlayoffs = isPlayoffs;
                game.HomeTeamScore = homeScore;
                game.AwayTeamScore = awayScore;

                yield return game;
            }
        }

        private static bool LeaguesTableLoaded(HtmlDocument document)
        {
            // WAIT until the dynamic text is set
            foreach (var table in document.DocumentNode.Descendants(HtmlTagNames.Table))
            {
                var attribute = table.GetAttributeValue(HtmlAttributes.Class, string.Empty);
                if (attribute == LeaguesTableClassAttribute)
                {
                    return !string.IsNullOrEmpty(table.InnerHtml);
                }
            }

            return false;
        }

        private static bool TournamentTableDivLoaded(HtmlDocument document)
        {
            var table = document.GetElementbyId("tournamentTable");
            if (table == null)
                return false;

            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(table.InnerText) &&
                !table.InnerText.ToLower().Equals("no data available");
        }        
    }
}
