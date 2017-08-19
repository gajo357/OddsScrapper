using Microsoft.EntityFrameworkCore;

namespace OddsWebsite.Models
{
    public class ArchiveContext : DbContext
    {
        public ArchiveContext(DbContextOptions<ArchiveContext> options)
            : base(options)
        {

        }

        public DbSet<LeagueInfo> Leagues { get; set; }
        public DbSet<GameInfo> Games { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ArchiveDataDb;Trusted_Connection=true;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction: b => b.MigrationsAssembly("OddsWebsite"));
        }
    }
}
