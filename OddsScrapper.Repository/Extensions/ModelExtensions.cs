using OddsScrapper.Repository.DbBuilder;
using OddsScrapper.Repository.Helpers;
using OddsScrapper.Repository.Models;
using OddsScrapper.Repository.Repository;

namespace OddsScrapper.Repository.Extensions
{
    public static class ModelExtensions
    {
        private static string[] CupNames = new[] { "cup", "copa", "cupen", "coupe", "coppa" };
        private const string Women = "women";

        public static bool IsWomen(this League league)
        {
            return league.Name.Contains(Women);
        }

        public static bool IsCup(this League league)
        {
            foreach (var cup in CupNames)
            {
                if (league.Name.Contains(cup))
                    return true;
            }

            return false;
        }

        public static int GetResult(this Game game)
        {
            if (game.HomeTeamScore > game.AwayTeamScore)
                return 1;

            if (game.HomeTeamScore < game.AwayTeamScore)
                return 2;

            return 0;
        }

        public static ColumnValuePair[] CreateColumnValuePairs(this Game game)
        {
            return new[]
            {
                ColumnValuePair.Create(new ForegnKeyTableColumn(DbRepository.GamesTable, DbRepository.LeaguesTable, "Id").ColumnName, game.League.Id),
                ColumnValuePair.Create(new ForegnKeyTableColumn(DbRepository.GamesTable, DbRepository.TeamsTable, "HomeTeamId").ColumnName, game.HomeTeam.Id),
                ColumnValuePair.Create(new ForegnKeyTableColumn(DbRepository.GamesTable, DbRepository.TeamsTable, "AwayTeamId").ColumnName, game.AwayTeam.Id),
                ColumnValuePair.Create(nameof(Game.Date), game.Date),
                ColumnValuePair.Create(nameof(Game.HomeTeamScore), game.HomeTeamScore),
                ColumnValuePair.Create(nameof(Game.AwayTeamScore), game.AwayTeamScore),
                ColumnValuePair.Create(nameof(Game.IsPlayoffs), game.IsPlayoffs),
                ColumnValuePair.Create(nameof(Game.IsOvertime), game.IsOvertime),
                ColumnValuePair.Create(nameof(Game.Season), game.Season),
                ColumnValuePair.Create(nameof(Game.GameLink), game.GameLink)
            };
        }

        public static ColumnValuePair[] CreateColumnValuePairs(this GameOdds odd, int gameId)
        {
            return new[]
                {
                    ColumnValuePair.Create(new ForegnKeyTableColumn(DbRepository.GameOddsTable, DbRepository.GamesTable, "Id").ColumnName, gameId),
                    ColumnValuePair.Create(new ForegnKeyTableColumn(DbRepository.GameOddsTable, DbRepository.BookersTable, "Id").ColumnName, odd.Bookkeeper.Id),
                    ColumnValuePair.Create(nameof(GameOdds.HomeOdd), odd.HomeOdd),
                    ColumnValuePair.Create(nameof(GameOdds.DrawOdd), odd.DrawOdd),
                    ColumnValuePair.Create(nameof(GameOdds.AwayOdd), odd.AwayOdd),
                    ColumnValuePair.Create(nameof(GameOdds.IsValid), odd.IsValid)
                };
        }
    }
}
