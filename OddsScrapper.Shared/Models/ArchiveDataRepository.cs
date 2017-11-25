using System.Collections.Generic;
using System.Linq;

namespace OddsWebsite.Models
{
    public class ArchiveDataRepository : IArchiveDataRepository
    {
        private ArchiveContext Context { get; }

        public ArchiveDataRepository(ArchiveContext context)
        {
            Context = context;
        }

        public IEnumerable<League> GetAllLeagues()
        {
            return Context.Leagues.ToList();
        }

        public IEnumerable<Game> GetAllGames()
        {
            return Context.Games.ToList();
        }

        public IEnumerable<Sport> GetAllSports()
        {
            return Context.Sports.ToList();
        }

        public IEnumerable<Country> GetAllCountries()
        {
            return Context.Countries.ToList();
        }
    }
}
