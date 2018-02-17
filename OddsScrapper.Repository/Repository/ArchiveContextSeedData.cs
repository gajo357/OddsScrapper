using System.Linq;
using System.Threading.Tasks;

namespace OddsScrapper.Repository.Repository
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
