using OddsScrapper.Repository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OddsScrapper.Repository.Repository
{
    public interface IDbRepository
    {
        Task<Team> GetOrCreateTeamAsync(string name, Sport sport);
        Task<Sport> GetSportAsync(string name);
        Task<Sport> GetOrCreateSportAsync(string name);
        Task<Country> GetCountryAsync(string name);
        Task<Country> GetOrCreateCountryAsync(string name);
        Task<IEnumerable<League>> GetLeaguesAsync(Sport sport, Country country);
        Task<League> GetLeagueAsync(string name, Sport sport, Country country);
        Task<League> GetOrCreateLeagueAsync(string name, bool isFirst, Sport sport, Country country);
        Task<Bookkeeper> GetOrCreateBookerAsync(string bookersName);
        Task<int> InsertGameAsync(Game game);
        Task UpdateGameAsync(Game game);
        Task<Game> UpdateOrInsertGameAsync(Game game);
        Task<bool> GameExistsAsync(Team homeTeam, Team awayTeam, DateTime date);
        Task<bool> GameExistsAsync(string gameLink);

        IEnumerable<Game> GetAllGames();
        Task<IEnumerable<Game>> GetAllLeagueGamesAsync(League league);
        IEnumerable<Game> GetAllLeagueGames(League league);

        int GetIdOfLastLeague();
    }
}