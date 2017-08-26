using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OddsWebsite.Models
{
    public class ArchiveContext : IdentityDbContext<OddsAppUser>
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionString = "Data Source=ArchiveData.db";
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
