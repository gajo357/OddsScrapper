using System.Collections.Generic;
using System.Linq;

namespace OddsWebsite.Models
{
    public class ArchiveDataRepository : IArchiveDataRepository
    {
        public ArchiveDataRepository(ArchiveContext context)
        {
            Context = context;
        }

        private ArchiveContext Context { get; }

        public IEnumerable<League> GetAllLeagues()
        {
            return Context.Leagues.ToList();
        }
    }
}
