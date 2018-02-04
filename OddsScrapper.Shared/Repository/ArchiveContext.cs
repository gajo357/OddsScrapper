using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OddsScrapper.Shared.Models;

namespace OddsScrapper.Shared.Repository
{
    public class ArchiveContextFactory : IDesignTimeDbContextFactory<ArchiveContext>
    {
        public ArchiveContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ArchiveContext>();
            return new ArchiveContext(optionsBuilder.Options);
        }
    }

    public class ArchiveContext : DbContext
    {
        public ArchiveContext(DbContextOptions<ArchiveContext> options)
            : base(options)
        {
        }

        public DbSet<League> Leagues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Bookkeeper> Bookers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionString = "Data Source=../OddsDataArchive.db";
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
