using System.Collections.Generic;
using OddsScrapper.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using OddsScrapper.Shared.Repository;

namespace OddsScrapper.Repository.Repository
{
    public class ArchiveDataRepository : IArchiveDataRepository
    {
        private ArchiveContext Context { get; }

        public ArchiveDataRepository(ArchiveContext context)
        {
            Context = context;
        }

        public async Task<IEnumerable<League>> GetAllLeaguesAsync()
        {
            return await Context.Leagues.ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetAllGamesAsync()
        {
            return await Context.Games.ToListAsync();
        }

        public async Task<IEnumerable<Sport>> GetAllSportsAsync()
        {
            return await Context.Sports.ToListAsync();
        }

        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            return await Context.Countries.ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetAllTeamsAsync()
        {
            return await Context.Teams.ToListAsync();
        }

        public async Task<IEnumerable<Bookkeeper>> GetAllBookersAsync()
        {
            return await Context.Bookers.ToListAsync();
        }

        public async Task<Team> GetTeamAsync(string name)
        {
            return await Context.Teams.SingleOrDefaultAsync(s => s.Name == name);
        }

        public async Task<Sport> GetSportAsync(string name)
        {
            return await Context.Sports.SingleOrDefaultAsync(s => s.Name == name);
        }

        public async Task<Country> GetCountryAsync(string name)
        {
            return await Context.Countries.SingleOrDefaultAsync(s => s.Name == name);
        }

        public async Task<Bookkeeper> GetBookkeeperAsync(string name)
        {
            return await Context.Bookers.SingleOrDefaultAsync(s => s.Name == name);
        }

        public async Task<League> GetLeagueAsync(string sportName, string countryName, string leagueName)
        {
            return await Context.Leagues
                .Include(s => s.Sport)
                .Include(s => s.Country)
                .SingleOrDefaultAsync(s => s.Name == leagueName && s.Sport.Name == sportName && s.Country.Name == countryName);
        }

        public async Task<Team> GetOrCreateTeamAsync(string teamName)
        {
            var team = await GetTeamAsync(teamName);
            if (team != null)
                return team;

            team = new Team() { Name = teamName };
            var result = await Context.Teams.AddAsync(team);

            return result.Entity;
        }

        public async Task<Sport> GetOrCreateSportAsync(string sportName)
        {
            var sport = await GetSportAsync(sportName);
            if (sport != null)
                return sport;

            sport = new Sport() { Name = sportName };
            var result = await Context.Sports.AddAsync(sport);

            return result.Entity;
        }

        public async Task<Country> GetOrCreateCountryAsync(string countryName)
        {
            var country = await GetCountryAsync(countryName);
            if (country != null)
                return country;

            country = new Country() { Name = countryName };
            var result = await Context.Countries.AddAsync(country);

            return result.Entity;
        }

        public async Task<League> GetOrCreateLeagueAsync(string sportName, string countryName, string leagueName)
        {
            var existingLeague = await GetLeagueAsync(sportName, countryName, leagueName);
            if (existingLeague != null)
                return existingLeague;

            var entry = new League() { Name = leagueName };
            entry.Country = await GetOrCreateCountryAsync(countryName);
            entry.Sport = await GetOrCreateSportAsync(sportName);

            var result = await Context.Leagues.AddAsync(entry);
            return result.Entity;
        }

        public async Task<Bookkeeper> GetOrCreateBookerAsync(string bookersName)
        {
            var booker = await GetBookkeeperAsync(bookersName);
            if (booker != null)
                return booker;

            booker = new Bookkeeper() { Name = bookersName };
            var result = await Context.Bookers.AddAsync(booker);

            return result.Entity;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public async Task InsertGamesAsync(IEnumerable<Game> games)
        {
            await Context.Games.AddRangeAsync(games);
        }
    }
}
