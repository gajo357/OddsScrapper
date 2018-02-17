using HtmlAgilityPack;
using OddsScrapper.Shared.Models;
using OddsScrapper.Shared.Repository;
using OddsScrapper.WebsiteScraping.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Scrappers
{
    public abstract class BaseScrapper
    {
        protected const string DateFormat = "dd MMM yyyy";
        protected const string TimeFormat = "HH:MM";

        protected IHtmlContentReader Reader { get; }
        protected IArchiveDataRepository Repository { get; }

        protected BaseScrapper(IArchiveDataRepository repository, IHtmlContentReader reader)
        {
            Repository = repository;
            Reader = reader;
        }

        protected (string countryName, string leagueName) GetLeagueAndCountryName(string sportName, string gameLink)
        {
            gameLink = gameLink.Substring(gameLink.IndexOf($"/{sportName}/"));
            var gameLinkParts = gameLink.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            //var gameSport = gameLinkParts[0];
            var countryName = gameLinkParts[1];
            var leagueName = gameLinkParts[2];
            return (countryName, leagueName);
        }

        protected bool HasGameFinished(HtmlNode[] tds)
        {
            var oddNodes = tds.Where(s => s.Attributes[HtmlAttributes.Class].Value.Contains("odds-nowrp"));

            return oddNodes.Any(s => s.Attributes[HtmlAttributes.Class].Value.Contains("result-ok"));
        }

        protected async Task<IEnumerable<Game>> GetGamesFromDocumentAsync(HtmlDocument gamesDocument, string sportName, bool getFinishedGames)
        {
            var games = new List<Game>();

            var div = gamesDocument.GetElementbyId("table-matches");
            var gamesTable = div.Element(HtmlTagNames.Table);
            var isPlayoffs = false;
            foreach (var tableRow in gamesTable.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                var attribute = tableRow.GetAttributeValue(HtmlAttributes.Class, null);
                if (string.IsNullOrEmpty(attribute))
                    continue;

                if (attribute.Contains("center nob-border"))
                {
                    isPlayoffs = tableRow.InnerText.Contains("Play Offs");
                }

                // date, matchup and odds tds in a row
                var tds = tableRow.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                if (tds.Length < 4)
                    continue;

                // game has finished?
                var gameFinished = HasGameFinished(tds);
                if (getFinishedGames && ! gameFinished)
                    continue;
                if (!getFinishedGames && gameFinished)
                    continue;

                var gameLink = ReadGameLink(tds);
                if (!gameLink.Contains(sportName))
                    continue;

                var gameDocument = await Reader.GetHtmlFromWebpageAsync(gameLink);
                if (gameDocument == null)
                    continue;

                var odds = await ReadGameOddsAsync(gameDocument);
                if (!odds.Any())
                    continue;

                var gameDate = ReadDateAndTime(gameDocument);

                (string countryName, string leagueName) = GetLeagueAndCountryName(sportName, gameLink);
                (int homeScore, int awayScore) = ReadGameScores(gameDocument);
                (string homeTeamName, string awayTeamName) = ReadParticipants(gameDocument);


                var homeTeam = await Repository.GetOrCreateTeamAsync(homeTeamName);
                var awayTeam = await Repository.GetOrCreateTeamAsync(awayTeamName);
                var sport = await Repository.GetOrCreateSportAsync(sportName);
                var country = await Repository.GetOrCreateCountryAsync(countryName);
                var league = await Repository.GetOrCreateLeagueAsync(sportName, countryName, leagueName);

                var game = new Game();
                game.IsPlayoffs = isPlayoffs;
                game.HomeTeam = homeTeam;
                game.AwayTeam = awayTeam;
                game.League = league;
                game.Odds.AddRange(odds);
                game.Date = gameDate;

                games.Add(game);
            }

            return games;
        }

        private (int homeScore, int awayScore) ReadGameScores(HtmlDocument gameDocument)
        {
            (int h, int a) defaultResult = (-1, -1);

            var contentDiv = gameDocument.GetElementbyId("event-status");
            if (contentDiv == null)
                return defaultResult;

            var statusText = contentDiv.InnerText;

            const string regexPattern = @"Final result (\d+):(\d+)*";
            var match = Regex.Match(statusText, regexPattern);
            if (match == null)
                return defaultResult;

            var home = Convert.ToInt32(match.Groups[1].Value);
            var away = Convert.ToInt32(match.Groups[2].Value);
            return (home, away);
        }

        protected string ReadGameLink(HtmlNode[] tds)
        {
            var nameTd = tds.First(s => s.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("table-participant"));
            var nameElement = nameTd.Elements(HtmlTagNames.A).First(s => !string.IsNullOrEmpty(s.GetAttributeValue(HtmlAttributes.Href, null)) && !s.Attributes[HtmlAttributes.Href].Value.Contains("javascript"));

            var gameLink = nameElement.Attributes[HtmlAttributes.Href].Value;

            return gameLink;
        }

        protected DateTime? ReadDateAndTime(HtmlDocument gameDocument)
        {
            HtmlNode dateNode = null;

            var contentDiv = gameDocument.GetElementbyId("col-content");
            foreach (var p in contentDiv.Elements("p"))
            {
                if (!p.GetAttributeValue(HtmlAttributes.Class, "").Contains("date"))
                {
                    continue;
                }

                dateNode = p;
                break;
            }

            if (dateNode == null)
                return null;

            var dateStrings = dateNode.InnerText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var dateString = dateStrings[1];
            var timeString = dateStrings[2];

            if (DateTime.TryParseExact($"{dateString} {timeString}", $"{DateFormat} {TimeFormat}", null, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime date))
                return date;

            return null;
        }

        protected (string home, string away) ReadParticipants(HtmlDocument gameDocument)
        {
            var contentDiv = gameDocument.GetElementbyId("col-content");
            var header = contentDiv.Element(HtmlTagNames.H1);

            var participants = header.InnerText.Replace("&nbsp;", string.Empty).Replace("&amp;", "and").Replace("'", " ").Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (participants.Length < 2)
                return (null, null);

            return (participants[0], participants[1]);
        }

        protected async Task<List<GameOdds>> ReadGameOddsAsync(HtmlDocument gameDocument)
        {
            var odds = new List<GameOdds>();

            HtmlNode table = GetOddsTableFromGameDocument(gameDocument);
            if (table == null)
                return odds;

            foreach (var tableRow in table.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                // date, matchup and odds tds in a row
                var tds = tableRow.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                if (tds.Length < 4)
                    continue;

                var bookersName = tds[0].InnerText.Replace("&nbsp;", string.Empty).Replace(Environment.NewLine, string.Empty);
                if (string.IsNullOrEmpty(bookersName))
                    continue;

                var booker = await Repository.GetOrCreateBookerAsync(bookersName);

                var oddsTds = tds.Where(t => t.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("right odds")).ToArray();
                if (oddsTds.Length < 2)
                    continue;

                var deactivated = oddsTds.Any(s => s.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("deactivate"));

                var currentOdds = oddsTds.Select(GetOddFromTdNode).ToArray();

                var odd = new GameOdds();
                odd.Bookkeeper = booker;
                odd.IsValid = !deactivated;
                odd.HomeOdd = currentOdds[0];
                if (currentOdds.Length == 2)
                {
                    odd.DrawOdd = 0;
                    odd.AwayOdd = currentOdds[1];
                }
                else
                {
                    odd.DrawOdd = currentOdds[1];
                    odd.AwayOdd = currentOdds[2];
                }

                odds.Add(odd);
            }

            return odds;
        }

        protected HtmlNode GetOddsTableFromGameDocument(HtmlDocument gameDocument)
        {
            var div = gameDocument.GetElementbyId("odds-data-table");
            HtmlNode table = null;
            foreach (var child in div.ChildNodes)
            {
                var t = child.Element(HtmlTagNames.Table);
                if (t != null && t.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("table-main detail-odds sortable"))
                {
                    table = t;
                    break;
                }
            }

            return table;
        }

        private static double GetOddFromTdNode(HtmlNode tdNode)
        {
            double odd;
            if (double.TryParse(tdNode.FirstChild.InnerText, out odd))
                return odd;

            return double.NaN;
        }
    }
}
