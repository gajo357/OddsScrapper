using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace OddsScrapper.DbModels
{
    public class DbRepository
    {
        private const string CountriesTable = "Countries";
        private const string SportsTable = "Sports";
        private const string LeaguesTable = "Leagues";
        private const string TeamsTable = "Teams";
        private const string GamesTable = "Games";

        private SQLiteConnection _sqlConnection;

        public DbRepository(string path)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path
            };

            _sqlConnection = new SQLiteConnection()
            {
                ConnectionString = builder.ConnectionString
            };
            _sqlConnection.Open();
        }

        public void Insert(string commandText)
        {
            using (var transaction = _sqlConnection.BeginTransaction())
            {
                using (var command = _sqlConnection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public int GetId(string commandText)
        {
            using (var command = _sqlConnection.CreateCommand())
            {
                command.CommandText = commandText;
                using (var reader = command.ExecuteReader())
                {
                    // Always call Read before accessing data.
                    while (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }

            return 0;
        }

        public void Close()
        {
            _sqlConnection.Close();
            _sqlConnection.Dispose();
            _sqlConnection = null;
        }

        internal int GetSportId(string sport)
        {
            var command = $"SELECT Id FROM {SportsTable} WHERE Name='{sport}'";
            return GetId(command);
        }

        internal int GetOrCreateCountryId(string country)
        {
            var getCommand = GetCoutrnyIdCommand(country);
            var insertCommand = $"INSERT INTO {CountriesTable} (Name) VALUES ('{country}');";
            return GetOrCreateId(getCommand, insertCommand);
        }

        internal int GetCountryId(string country)
        {
            var getCommand = GetCoutrnyIdCommand(country);
            return GetId(getCommand);
        }

        private string GetCoutrnyIdCommand(string country)
        {
            return $"SELECT Id FROM {CountriesTable} WHERE Name='{country}';";
        }

        private int GetOrCreateTeamId(int leagueId, string team)
        {
            var getCommand = GetTeamIdCommand(leagueId, team);
            var insertCommand = $"INSERT INTO {TeamsTable} (Name,LeagueId) VALUES ('{team}','{leagueId}');";

            return GetOrCreateId(getCommand, insertCommand);
        }

        public int GetTeamId(int leagueId, string team)
        {
            var getCommand = GetTeamIdCommand(leagueId, team);

            return GetId(getCommand);
        }

        private string GetTeamIdCommand(int leagueId, string team)
        {
            return $"SELECT Id FROM {TeamsTable} WHERE Name='{team}' AND LeagueId='{leagueId}';";
        }

        public int GetOrCreateLeagueId(int sportId, int countryId, LeagueInfo leagueInfo)
        {
            var name = leagueInfo.Name;
            var isFirst = leagueInfo.IsFirst ? 1 : 0;
            var isWomen = leagueInfo.IsWomen ? 1 : 0;
            var isCup = leagueInfo.IsCup ? 1 : 0;

            var getCommand = GetLeagueCommand(sportId, countryId, name);
            var insertCommand = $"INSERT INTO {LeaguesTable} (Name,CountryId,SportId,IsFirst,IsWomen,IsCup) VALUES ('{name}','{countryId}','{sportId}','{isFirst}','{isWomen}','{isCup}');";

            return GetOrCreateId(getCommand, insertCommand);
        }

        public LeagueInfo GetLeague(int sportId, int countryId, string name)
        {
            var commandText = $"SELECT Id,IsFirst,IsWomen,IsCup FROM {LeaguesTable} WHERE Name='{name}' AND SportId='{sportId}' AND CountryId='{countryId}';";

            using (var command = _sqlConnection.CreateCommand())
            {
                command.CommandText = commandText;
                using (var reader = command.ExecuteReader())
                {
                    // Always call Read before accessing data.
                    while (reader.Read())
                    {
                        var league = new LeagueInfo();
                        league.Id = reader.GetInt32(0);
                        league.IsFirst = reader.GetInt32(1) == 1;
                        league.IsWomen = reader.GetInt32(2) == 1;
                        league.IsCup = reader.GetInt32(3) == 1;

                        return league;
                    }
                }
            }

            return null;
        }

        private string GetLeagueCommand(int sportId, int countryId, string name)
        {
            return $"SELECT Id FROM {LeaguesTable} WHERE Name='{name}' AND SportId='{sportId}' AND CountryId='{countryId}';";
        }

        private int GetOrCreateId(string getCommand, string insertCommand)
        {
            var id = GetId(getCommand);
            if (id > 0)
                return id;

            Insert(insertCommand);

            return GetId(getCommand);
        }

        public void InsertGames(int leagueId, int season, IEnumerable<Game> games)
        {
            var commandText = new StringBuilder();
            foreach (var game in games)
            {
                var isOvertime = game.IsOvertime ? 1 : 0;
                var isPlayoffs = game.IsPlayoffs ? 1 : 0;
                var homeTeamId = GetOrCreateTeamId(leagueId, game.HomeTeam);
                var awayTeamId = GetOrCreateTeamId(leagueId, game.AwayTeam);

                var insertCommand = $"INSERT INTO {GamesTable} (LeagueId,IsPlayoffs,Season,Date,HomeTeamId,AwayTeamId,HomeTeamScore,AwayTeamScore,IsOvertime,HomeOdd,DrawOdd,AwayOdd,Bet,Winner) VALUES ('{leagueId}','{isPlayoffs}','{season}','{game.Date}','{homeTeamId}','{awayTeamId}','{game.HomeTeamScore}','{game.AwayTeamScore}','{isOvertime}','{game.HomeOdd}','{game.DrawOdd}','{game.AwayOdd}','{game.Bet}','{game.Winner}');";
                commandText.AppendLine(insertCommand);
            }

            Insert(commandText.ToString());
        }

        ~DbRepository()
        {
            Close();
        }
    }
}
