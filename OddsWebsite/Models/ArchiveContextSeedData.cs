using OddsWebsite.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
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

            var leagues = ReadLeaguesInfos();
            CollectLeaguesData(leagues);

            await ArchiveContext.SaveChangesAsync();
        }

        private IDictionary<Tuple<string, string, string>, League> ReadLeaguesInfos()
        {
            var dataDirectory = FolderStructureHelpers.GetArchiveFolderPath();
            var leaguesInfoFiles = Directory.GetFiles(dataDirectory, "*.txt");

            var sports = new Dictionary<string, Sport>();
            var countries = new Dictionary<string, Country>();

            var result = new Dictionary<Tuple<string, string, string>, League>();
            foreach (var fileName in leaguesInfoFiles)
            {
                foreach (var line in File.ReadLines(fileName))
                {
                    var data = line.Split(',');
                    var sportName = data[0];
                    if (sportName == "Sport")
                        continue;

                    var countryName = data[1];
                    var leagueName = data[2];
                    var isFirst = int.Parse(data[3]) == 1;

                    if(!sports.ContainsKey(sportName))
                    {
                        var newSport = new Sport() { Name = sportName };
                        ArchiveContext.Sports.Add(newSport);
                        sports.Add(sportName, newSport);
                    }
                    var sport = sports[sportName];

                    if (!countries.ContainsKey(countryName))
                    {
                        var newCountry = new Country() { Name = countryName };
                        ArchiveContext.Countries.Add(newCountry);
                        countries.Add(countryName, newCountry);
                    }
                    var country = countries[countryName];

                    var key = new Tuple<string, string, string>(sportName, countryName, leagueName);
                    if (!result.ContainsKey(key))
                    {
                        var league = new League();
                        league.Sport = sport;
                        league.Country = country;
                        league.Name = leagueName;
                        league.IsFirst = isFirst;
                        league.Games = new List<Game>();
                        league.Teams = new List<Team>();

                        result.Add(key, league);
                        ArchiveContext.Leagues.Add(league);
                    }
                }
            }

            return result;
        }

        private void CollectLeaguesData(IDictionary<Tuple<string, string, string>, League> leaguesInfo)
        {
            var dataDirectory = FolderStructureHelpers.GetArchiveFolderPath();
            var files = Directory.GetFiles(dataDirectory, "*.csv");

            var teams = new Dictionary<Tuple<string, string, string, string>, Team>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                var sport = fileName[0];
                var country = fileName[1];
                var leagueName = fileName[2];

                var key = new Tuple<string, string, string>(sport, country, leagueName);
                if (!leaguesInfo.ContainsKey(key))
                    continue;

                var league = leaguesInfo[key];

                foreach (var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    if (data.Length != 5)
                        continue;

                    var season = data[0];
                    if (season == "Season")
                        continue;

                    var participants = data[1].Replace("&nbsp;", string.Empty).Split(" - ", StringSplitOptions.RemoveEmptyEntries);
                    var odd = double.Parse(data[2]);
                    var bestBet = int.Parse(data[3]);
                    var winBet = int.Parse(data[4]);
                    var success = bestBet == winBet;

                    var homeTeamName = participants.First().Trim();
                    var awayTeamName = participants.Last().Trim();
                    var homeTeam = GetOrCreateTeam(teams, sport, country, leagueName, league, homeTeamName);
                    var awayTeam = GetOrCreateTeam(teams, sport, country, leagueName, league, awayTeamName);

                    var game = new Game();
                    game.HomeTeam = homeTeam;
                    game.AwayTeam = awayTeam;
                    game.WinningOdd = odd;
                    game.Winner = winBet;
                    game.Bet = bestBet;
                    game.Season = int.Parse(season);
                    game.Date = new DateTime(game.Season, 1, 1);

                    league.Games.Add(game);
                    ArchiveContext.Games.Add(game);
                }
            }
        }

        private Team GetOrCreateTeam(Dictionary<Tuple<string, string, string, string>, Team> teams, string sport, string country, string leagueName, League league, string teamName)
        {
            var teamKey = new Tuple<string, string, string, string>(sport, country, leagueName, teamName);
            if (!teams.ContainsKey(teamKey))
            {
                var newTeam = new Team() { Name = teamName };

                teams.Add(teamKey, newTeam);
                ArchiveContext.Teams.Add(newTeam);
                league.Teams.Add(newTeam);
            }
            return teams[teamKey];
        }
    }
}
