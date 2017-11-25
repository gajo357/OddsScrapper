using System.Collections.Generic;

namespace OddsWebsite.Models
{
    public interface IArchiveDataRepository
    {
        IEnumerable<Country> GetAllCountries();
        IEnumerable<Game> GetAllGames();
        IEnumerable<League> GetAllLeagues();
        IEnumerable<Sport> GetAllSports();
    }
}