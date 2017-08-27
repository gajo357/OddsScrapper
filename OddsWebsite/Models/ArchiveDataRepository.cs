using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OddsWebsite.Models
{
    public class ArchiveDataRepository : IArchiveDataRepository
    {
        private IDictionary<Tuple<string, string, string, int>, LeagueProcessedData> _analysedLeagueData = new Dictionary<Tuple<string, string, string, int>, LeagueProcessedData>();

        public ArchiveDataRepository(ArchiveContext context)
        {
            Context = context;
        }

        private ArchiveContext Context { get; }

        public IEnumerable<League> GetAllLeagues()
        {
            return Context.Leagues.ToList();
        }

        public LeagueProcessedData GetLeagueData(string sport, string country, string leagueName, double odd)
        {
            var category = OddToCategory(odd);

            var key = new Tuple<string, string, string, int>(sport, country, leagueName, category);
            if (!_analysedLeagueData.ContainsKey(key))
            {
                // analyse the league
                var league = Context.Leagues
                    .Include(s => s.Sport)
                    .Include(s => s.Country)
                    .Include(s => s.Games)
                    .FirstOrDefault(s => s.Sport.Name == sport && s.Country.Name == country && s.Name == leagueName);
                if (league == null)
                    return null;

                var games = league.Games.Where(s => category == OddToCategory(s.WinningOdd));
                var leagueData = new LeagueProcessedData();
                leagueData.AddGames(games);

                _analysedLeagueData.Add(new KeyValuePair<Tuple<string, string, string, int>, LeagueProcessedData>(key, leagueData));
            }

            return _analysedLeagueData[key];
        }

        public object GetResultsForUser(string name)
        {
            throw new NotImplementedException();
        }

        private int OddToCategory(double odd)
        {
            if (odd <= 1.1)
                return 1;
            if (odd <= 1.2)
                return 2;
            if (odd <= 1.3)
                return 3;
            if (odd <= 1.4)
                return 4;

            return 5;
        }
    }
}
