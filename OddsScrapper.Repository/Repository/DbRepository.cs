using OddsScrapper.Repository.DbBuilder;
using OddsScrapper.Repository.Extensions;
using OddsScrapper.Repository.Helpers;
using OddsScrapper.Repository.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace OddsScrapper.Repository.Repository
{
    public class DbRepository : IDbRepository, IDisposable
    {
        public const string CountriesTable = "Countries";
        public const string SportsTable = "Sports";
        public const string LeaguesTable = "Leagues";
        public const string TeamsTable = "Teams";
        public const string GamesTable = "Games";
        public const string GameOddsTable = "GameOdds";
        public const string BookersTable = "Bookkeepers";

        private SQLiteConnection _sqlConnection;

        public DbRepository(string path)
        {
            var file = new FileInfo(path);
            var buildDb = !file.Exists;

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder
            {
                DataSource = file.FullName
            };

            _sqlConnection = new SQLiteConnection()
            {
                ConnectionString = builder.ConnectionString
            };
            _sqlConnection.Open();

            if (buildDb)
                CreateTables();
        }

        private void CreateTables()
        {
            var sportsTable = TableBuilder.CreateSportTable();
            var countriesTable = TableBuilder.CreateCountriesTable();
            var bookTable = TableBuilder.CreateBookerTable();
            var leagueTable = TableBuilder.CreateLeagueTable();
            var teamTable = TableBuilder.CreateTeamTable();
            var gameTable = TableBuilder.CreateGameTable();
            var gameOddsTable = TableBuilder.CreateGameOddsTable();

            _sqlConnection.Create(sportsTable, countriesTable, leagueTable, bookTable, teamTable, gameTable, gameOddsTable);
        }

        public void Close()
        {
            _sqlConnection.Close();
            _sqlConnection.Dispose();
            _sqlConnection = null;
        }

        private async Task<Sport> GetSportAsync(int id)
        {
            return await _sqlConnection.GetByIdAsync(SportsTable, id, CreateSportAsync);
        }
        private Task<Sport> CreateSportAsync(DbDataReader reader)
        {
            return Task.FromResult(
                new Sport
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
        }

        public async Task<Sport> GetSportAsync(string name)
        {
            var dbId = await _sqlConnection.GetIdAsync(SportsTable, ColumnValuePair.CreateName(name));
            if (dbId > 0)
            {
                return new Sport
                {
                    Id = dbId,
                    Name = name
                };
            }

            return null;
        }
        private async Task<Sport> CreateSportAsync(string name)
        {
            var id = await _sqlConnection.InsertAsync(SportsTable, ColumnValuePair.CreateName(name));
            return new Sport()
            {
                Id = id,
                Name = name
            };
        }
        public async Task<Sport> GetOrCreateSportAsync(string name)
        {
            return await GetSportAsync(name) ?? await CreateSportAsync(name);
        }
        
        private async Task<Country> GetCountryAsync(int id)
        {
            return await _sqlConnection.GetByIdAsync(CountriesTable, id, CreateCountryAsync);
        }
        private Task<Country> CreateCountryAsync(DbDataReader reader)
        {
            return Task.FromResult(
                new Country
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
        }

        public async Task<Country> GetCountryAsync(string name)
        {
            var dbId = await _sqlConnection.GetIdAsync(CountriesTable, ColumnValuePair.CreateName(name));
            if (dbId > 0)
            {
                return new Country
                {
                    Id = dbId,
                    Name = name
                };
            }

            return null;
        }
        private async Task<Country> CreateCountryAsync(string name)
        {
            var id = await _sqlConnection.InsertAsync(CountriesTable, ColumnValuePair.CreateName(name));
            return new Country()
            {
                Id = id,
                Name = name
            };
        }
        public async Task<Country> GetOrCreateCountryAsync(string name)
        {
            return await GetCountryAsync(name) ?? await CreateCountryAsync(name);
        }

        private async Task<Team> GetTeamAsync(int id)
        {
            return await _sqlConnection.GetByIdAsync(TeamsTable, id, CreateTeamAsync);
        }
        private async Task<Team> CreateTeamAsync(DbDataReader reader)
        {
            return 
                new Team
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sport = await GetSportAsync(reader.GetInt32(2))
                };
        }

        public async Task<Team> GetTeamAsync(string name, Sport sport)
        {
            var dbId = await _sqlConnection.GetIdAsync(TeamsTable, 
                ColumnValuePair.CreateName(name), 
                ColumnValuePair.Create(new ForegnKeyTableColumn(TeamsTable, SportsTable, "Id").ColumnName, sport.Id));
            if (dbId > 0)
            {
                return new Team
                {
                    Id = dbId,
                    Name = name,
                    Sport = sport
                };
            }

            return null;
        }
        private async Task<Team> CreateTeamAsync(string name, Sport sport)
        {
            var id = await _sqlConnection.InsertAsync(TeamsTable, 
                ColumnValuePair.CreateName(name), 
                ColumnValuePair.Create(new ForegnKeyTableColumn(TeamsTable, SportsTable, "Id").ColumnName, sport.Id));

            return new Team()
            {
                Id = id,
                Name = name,
                Sport = sport
            };
        }
        public async Task<Team> GetOrCreateTeamAsync(string name, Sport sport)
        {
            return await GetTeamAsync(name, sport) ?? await CreateTeamAsync(name, sport);
        }

        private async Task<League> GetLeagueAsync(int id)
        {
            return await _sqlConnection.GetByIdAsync(LeaguesTable, id, CreateLeagueAsync);
        }
        private async Task<League> CreateLeagueAsync(DbDataReader reader)
        {
            var i = 0;

            return new League
            {
                Id = reader.GetInt32(i++),
                Name = reader.GetString(i++),
                IsFirst = reader.GetBoolean(i++),
                Sport = await GetSportAsync(reader.GetInt32(i++)),
                Country = await GetCountryAsync(reader.GetInt32(i++))
            };
        }

        public async Task<IEnumerable<League>> GetLeaguesAsync(Sport sport, Country country)
        {
            return await _sqlConnection.GetAllAsync(LeaguesTable,
                new[]
                {
                    ColumnValuePair.Create(new ForegnKeyTableColumn(LeaguesTable, SportsTable, "Id").ColumnName, sport.Id),
                    ColumnValuePair.Create(new ForegnKeyTableColumn(LeaguesTable, CountriesTable, "Id").ColumnName, country.Id)
                },
                CreateLeagueAsync);
        }
        public async Task<League> GetLeagueAsync(string name, Sport sport, Country country)
        {
            var dbId = await _sqlConnection.GetIdAsync(LeaguesTable,
                ColumnValuePair.CreateName(name),
                ColumnValuePair.Create(new ForegnKeyTableColumn(LeaguesTable, SportsTable, "Id").ColumnName, sport.Id),
                ColumnValuePair.Create(new ForegnKeyTableColumn(LeaguesTable, CountriesTable, "Id").ColumnName, country.Id));

            if (dbId > 0)
            {
                return new League
                {
                    Id = dbId,
                    Name = name,
                    Sport = sport,
                    Country = country
                };
            }

            return null;
        }
        private async Task<League> CreateLeagueAsync(string name, bool isFirst, Sport sport, Country country)
        {
            var id = await _sqlConnection.InsertAsync(LeaguesTable, 
                ColumnValuePair.CreateName(name), 
                ColumnValuePair.Create(new ForegnKeyTableColumn(LeaguesTable, SportsTable, "Id").ColumnName, sport.Id), 
                ColumnValuePair.Create(new ForegnKeyTableColumn(LeaguesTable, CountriesTable, "Id").ColumnName, country.Id), 
                ColumnValuePair.Create(nameof(League.IsFirst), isFirst));

            return new League()
            {
                Id = id,
                Name = name,
                Sport = sport,
                Country = country,
                IsFirst = isFirst
            };
        }
        public async Task<League> GetOrCreateLeagueAsync(string name, bool isFirst, Sport sport, Country country)
        {
            return await GetLeagueAsync(name, sport, country) ?? await CreateLeagueAsync(name, isFirst, sport, country);
        }

        private async Task<Bookkeeper> GetBookerAsync(int id)
        {
            return await _sqlConnection.GetByIdAsync(BookersTable, id, CreateBookerAsync);
        }
        private Task<Bookkeeper> CreateBookerAsync(DbDataReader reader)
        {
            return Task.FromResult(
                new Bookkeeper
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
        }

        private async Task<Bookkeeper> GetBookerAsync(string name)
        {
            var dbId = await _sqlConnection.GetIdAsync(BookersTable, ColumnValuePair.CreateName(name));
            if (dbId > 0)
            {
                return new Bookkeeper
                {
                    Id = dbId,
                    Name = name
                };
            }

            return null;
        }
        private async Task<Bookkeeper> CreateBookerAsync(string name)
        {
            var id = await _sqlConnection.InsertAsync(BookersTable, ColumnValuePair.CreateName(name));
            return new Bookkeeper()
            {
                Id = id,
                Name = name
            };
        }
        public async Task<Bookkeeper> GetOrCreateBookerAsync(string name)
        {
            return await GetBookerAsync(name) ?? await CreateBookerAsync(name);
        }

        public async Task<int> InsertGameAsync(Game game)
        {
            var gameId = await _sqlConnection.InsertAsync(GamesTable, game.CreateColumnValuePairs());
            await InsertGameOddsAsync(gameId, game.Odds);

            return gameId;
        }

        private async Task<int> UpdateGameAsync(int gameId, Game game)
        {
            game.Id = gameId;

            await _sqlConnection.UpdateAsync(GamesTable, gameId, game.CreateColumnValuePairs());
            await InsertGameOddsAsync(gameId, game.Odds);

            return gameId;
        }

        public async Task<Game> UpdateOrInsertGameAsync(Game game)
        {
            var gameId = await GetGameIdAsync(game.HomeTeam, game.AwayTeam, game.Date);

            if (gameId > 0)
            {
                await DeleteGameOddsAsync(gameId);
                await UpdateGameAsync(gameId, game);
            }
            else
            {
                await InsertGameAsync(game);
            }

            return game;
        }

        public async Task<bool> GameExistsAsync(Team homeTeam, Team awayTeam, DateTime date)
        {
            return await GetGameIdAsync(homeTeam, awayTeam, date) > 0;
        }

        public async Task<bool> GameExistsAsync(string gameLink)
        {
            return await _sqlConnection.GetIdAsync(GamesTable,
                ColumnValuePair.Create(nameof(Game.GameLink), gameLink)) > 0;
        }

        private async Task<int> GetGameIdAsync(Team homeTeam, Team awayTeam, DateTime? date)
        {
            return await _sqlConnection.GetIdAsync(GamesTable,
                ColumnValuePair.Create(new ForegnKeyTableColumn(GamesTable, TeamsTable, "HomeTeamId").ColumnName, homeTeam.Id),
                ColumnValuePair.Create(new ForegnKeyTableColumn(GamesTable, TeamsTable, "AwayTeamId").ColumnName, awayTeam.Id),
                ColumnValuePair.Create(nameof(Game.Date), date));
        }

        private async Task<int> DeleteGameOddsAsync(int gameId)
        {
            return await _sqlConnection.DeleteAsync(GameOddsTable, 
                ColumnValuePair.Create(new ForegnKeyTableColumn(GameOddsTable, GamesTable, "Id").ColumnName, gameId));
        }

        private async Task<int> InsertGameOddsAsync(int gameId, IList<GameOdds> gameOdds)
        {
            using (var transaction = _sqlConnection.BeginTransaction())
            {
                using (var command = _sqlConnection.CreateCommand())
                {
                    command.Transaction = transaction;

                    var columnValuePairs = gameOdds[0].CreateColumnValuePairs(gameId);
                    command.BuildInsertCommand(GameOddsTable, columnValuePairs);

                    foreach(var odd in gameOdds)
                    {
                        // not pretty, have to keep the same order of params as in 'columnValuePairs'
                        command.Parameters[0].Value = gameId;
                        command.Parameters[1].Value = odd.Bookkeeper.Id;
                        command.Parameters[2].Value = odd.HomeOdd;
                        command.Parameters[3].Value = odd.DrawOdd;
                        command.Parameters[4].Value = odd.AwayOdd;
                        command.Parameters[5].Value = odd.IsValid;

                        await command.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
            }

            return 1;
        }

        private async Task<IEnumerable<GameOdds>> GetGameOddsAsync(int gameId)
        {
            return await _sqlConnection.GetAllAsync(GameOddsTable,
                new[] { ColumnValuePair.Create(new ForegnKeyTableColumn(GameOddsTable, GamesTable, "Id").ColumnName, gameId) },
                CreateGameOddFromReaderAsync);
        }

        private async Task<GameOdds> CreateGameOddFromReaderAsync(DbDataReader reader)
        {
            var i = 1;

            return new GameOdds
            {
                Bookkeeper = await GetBookerAsync(reader.GetInt32(i++)),
                HomeOdd = reader.GetDouble(i++),
                DrawOdd = reader.GetDouble(i++),
                AwayOdd = reader.GetDouble(i++),
                IsValid = reader.GetBoolean(i++),
            };
        }

        public int GetIdOfLastLeague()
        {
            using (var command = _sqlConnection.CreateCommand())
            {
                command.CommandText = $"SELECT (Id) FROM {LeaguesTable} ORDER BY Id DESC LIMIT 1;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        return reader.GetInt32(0);
                }
            }

            return 0;
        }

        public void Dispose()
        {
            Close();
        }

        public async Task<IEnumerable<Game>> GetAllLeagueGamesAsync(League league)
        {
            return await _sqlConnection.GetAllAsync(GamesTable, 
                new[] { ColumnValuePair.Create(new ForegnKeyTableColumn(GamesTable, LeaguesTable, "Id").ColumnName, league.Id) },
                CreateGameAsync);
        }

        private async Task<Game> CreateGameAsync(DbDataReader reader)
        {
            var i = 0;

            var game = new Game
            {
                Id = reader.GetInt32(i++),
                Date = reader.GetDateTime(i++),
                HomeTeamScore = reader.GetInt32(i++),
                AwayTeamScore = reader.GetInt32(i++),
                IsPlayoffs = reader.GetBoolean(i++),
                IsOvertime = reader.GetBoolean(i++),
                Season = reader.GetString(i++),
                GameLink = reader.GetString(i++),
                League = await GetLeagueAsync(reader.GetInt32(i++)),
                HomeTeam = await GetTeamAsync(reader.GetInt32(i++)),
                AwayTeam = await GetTeamAsync(reader.GetInt32(i++)),
            };
            game.Odds.AddRange(await GetGameOddsAsync(game.Id));

            return game;
        }
    }
}