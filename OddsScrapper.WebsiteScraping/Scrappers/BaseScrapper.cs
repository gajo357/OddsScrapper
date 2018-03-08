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
    public abstract class BaseScrapper
    {
        public const string DateFormat = "dd MMM yyyy";
        public const string TimeFormat = "HH:MM";

        protected IHtmlContentReader Reader { get; }
        protected IDbRepository Repository { get; }

        protected BaseScrapper(IDbRepository repository, IHtmlContentReader reader)
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

        protected async Task<IEnumerable<Game>> GetGamesFromDocumentAsync(HtmlDocument gamesDocument, Sport sport, bool getFinishedGames, League league = null)
        {
            var games = new List<Game>();

            var div = gamesDocument.GetElementbyId("table-matches");
            var gamesTable = div.Elements(HtmlTagNames.Table).FirstOrDefault();
            if (gamesTable == null)
                return games;

            var isPlayoffs = false;
            foreach (var tableRow in gamesTable.Element(HtmlTagNames.Tbody).ChildNodes.WithAttribute(HtmlAttributes.Class))
            {
                var attribute = tableRow.Attributes[HtmlAttributes.Class];
                if (attribute.Value.Contains("center nob-border"))
                {
                    isPlayoffs = tableRow.InnerText.Contains("Play Offs");
                }

                // date, matchup and odds tds in a row
                var tds = tableRow.ChildNodes.WithName(HtmlTagNames.Td).ToArray();
                if (tds.Length < 4)
                    continue;

                // game has finished?
                var gameFinished = tds.HasGameFinished();
                if (getFinishedGames && ! gameFinished)
                    continue;
                if (!getFinishedGames && gameFinished)
                    continue;

                var gameLink = tds.ReadGameLink();
                if (!gameLink.Contains(sport.Name))
                    continue;

                var gameDocument = await Reader.GetHtmlFromWebpageAsync(gameLink);
                if (gameDocument == null)
                    continue;

                var odds = gameDocument.ReadGameOddsFromGameDocument();
                if (!odds.Any())
                    continue;

                var oddsTask = GetGameOddsWithBookersAsync(odds);

                var leagueTask = league == null ?
                    ReadLeague(sport, gameLink) :
                    Task.FromResult(league);

                var gameDate = gameDocument.ReadDateAndTimeFromGameDocument();
                (int homeScore, int awayScore, bool isOvertime) = gameDocument.ReadGameScoresFromGameDocument();
                (string homeTeamName, string awayTeamName) = gameDocument.ReadParticipantsFromGameDocument();

                var homeTeam = Repository.GetOrCreateTeamAsync(homeTeamName, sport);
                var awayTeam = Repository.GetOrCreateTeamAsync(awayTeamName, sport);

                var game = new Game();
                game.IsOvertime = isOvertime;
                game.IsPlayoffs = isPlayoffs;
                game.HomeTeam = await homeTeam;
                game.AwayTeam = await awayTeam;
                game.League = await leagueTask;
                game.Odds.AddRange(await oddsTask);
                game.Date = gameDate;

                games.Add(await Repository.UpdateOrInsertGameAsync(game));
            }

            return games;
        }

        private async Task<League> ReadLeague(Sport sport, string gameLink)
        {
            (string countryName, string leagueName) = GetLeagueAndCountryName(sport.Name, gameLink);
            var country = await Repository.GetOrCreateCountryAsync(countryName);
            return await Repository.GetOrCreateLeagueAsync(leagueName, false, sport, country);
        }

        protected async Task<IList<GameOdds>> GetGameOddsWithBookersAsync(IEnumerable<(string bookersName, GameOdds)> oddsWithName)
        {
            var result = new List<GameOdds>();

            foreach((string bookerName, GameOdds odd) in oddsWithName)
            {
                var booker = await Repository.GetOrCreateBookerAsync(bookerName);
                odd.Bookkeeper = booker;

                result.Add(odd);
            }

            return result;
        }
    }
}
