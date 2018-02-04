using OddsScrapper.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OddsScrapper.Shared.Repository
{
    public interface IArchiveDataRepository
    {
        Task<IEnumerable<Country>> GetAllCountriesAsync();
        Task<IEnumerable<Game>> GetAllGamesAsync();
        Task<IEnumerable<League>> GetAllLeaguesAsync();
        Task<IEnumerable<Sport>> GetAllSportsAsync();
        Task<Team> GetOrCreateTeamAsync(string name);
        Task<Sport> GetOrCreateSportAsync(string name);
        Task<Country> GetOrCreateCountryAsync(string name);
        Task<League> GetOrCreateLeagueAsync(string sportName, string countryName, string leagueName);
        Task<Bookkeeper> GetOrCreateBookerAsync(string bookersName);
        Task<int> SaveChangesAsync();
        Task InsertGamesAsync(IEnumerable<Game> games);
    }
}