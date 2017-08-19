using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OddsWebsite.Models
{
    public class ArchiveContextSeedData
    {
        public ArchiveContextSeedData(ArchiveContext archiveContext)
        {
            ArchiveContext = archiveContext;
        }

        private ArchiveContext ArchiveContext { get; }

        public async Task EnsureDataSeed()
        {
            if (ArchiveContext.Leagues.Any())
                return;

            ArchiveContext.Leagues.Add(new LeagueInfo());

            await ArchiveContext.SaveChangesAsync();
        }
    }
}
