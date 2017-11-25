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

        public async Task EnsureDataSeedAsync()
        {
            if (ArchiveContext.Leagues.Any())
                return;

            //CollectLeaguesData();

            await ArchiveContext.SaveChangesAsync();
        }
    }
}
