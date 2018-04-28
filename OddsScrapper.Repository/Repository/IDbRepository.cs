using OddsScrapper.Repository.Models;
using System;
using System.Threading.Tasks;

namespace OddsScrapper.Repository.Repository
{
    public interface IDbRepository
    {
        Task<Team> GetOrCreateTeamAsync(string name, Sport sport);
        Task<Sport> GetOrCreateSportAsync(string name);
        Task<Country> GetOrCreateCountryAsync(string name);
        Task<League> GetOrCreateLeagueAsync(string name, bool isFirst, Sport sport, Country country);
        Task<Bookkeeper> GetOrCreateBookerAsync(string bookersName);
        Task<int> InsertGameAsync(Game game);
        Task<Game> UpdateOrInsertGameAsync(Game game);
        Task<bool> GameExistsAsync(Team homeTeam, Team awayTeam, DateTime date);

        int GetIdOfLastLeague();
    }
}