using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class OddsMatcher
    {
        public void MatchGamesWithArchivedData()
        {
            var date = "15Aug2017";
            var archivedData = GetArchivedData();

            var games = GetAllTommorowsGames(date);

            MatchGames(archivedData, games, date);
        }

        private void MatchGames(LeagueTypeData[] archivedData, IEnumerable<GameInfo> games, string date)
        {
            var filteredGames = GetFilteredGames(archivedData, games);

            using (var fileStream = File.AppendText($"GamesToBet_{date}.csv"))
            {
                foreach (var game in filteredGames.OrderByDescending(s => s.MoneyPerGame))
                {
                    var line = $"{game.Sport},{game.Country},{game.League},{game.TotalRecords},{game.SuccessRate:F4},{game.MoneyPerGame:F4},{game.Participants},{String.Join(",", game.Odds.Select(s => s).ToArray())}";
                    fileStream.WriteLine(line);
                }
            }
        }

        private IEnumerable<GameInfo> GetFilteredGames(LeagueTypeData[] archivedData, IEnumerable<GameInfo> games)
        {
            foreach (var game in games)
            {
                var bestOdd = game.Odds.Min();
                LeagueTypeData league = null;

                foreach (var l in archivedData.Where(s => s.Info.Sport == game.Sport &&
                                                             s.Info.Country == game.Country &&
                                                             s.Info.Name == game.League &&
                                                             s.Margin > bestOdd))
                {
                    // find margin closest to the actual odd
                    // meaning minimal margin
                    if (league == null ||
                        league.Margin > l.Margin)
                        league = l;
                }

                if (league == null)
                    continue;

                game.MoneyPerGame = league.MoneyPerGame;
                game.TotalRecords = league.TotalRecords;
                game.SuccessRate = league.SuccessRate;

                yield return game;
            }
        }

        private IEnumerable<GameInfo> GetAllTommorowsGames(string gameDate)
        {
            var dataDirectory = HelperMethods.GetTommorowsGamesFolderPath();
            var files = Directory.GetFiles(dataDirectory, "*.csv");
            foreach(var file in files)
            {
                var date = Path.GetFileNameWithoutExtension(file).Split('_').Last();
                if (date != gameDate)
                    continue;

                foreach(var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    var sport = data[0];
                    var country = data[1];
                    var name = data[2];
                    var participants = data[3];
                    var odds = new List<double>();
                    for (var i = 4; i < data.Length; i++)
                        odds.Add(double.Parse(data[i]));

                    var game = new GameInfo()
                    {
                        Sport = sport,
                        Country = country,
                        League = name,
                        Participants = participants,
                        Odds = odds.ToArray()
                    };

                    yield return game;
                }
            }
        }

        private LeagueTypeData[] GetArchivedData()
        {
            var results = new List<LeagueTypeData>();
            foreach(var line in File.ReadLines(HelperMethods.GetAllAnalysedResultsFile()))
            {
                var data = line.Split(',');
                var sport = data[0];
                var country = data[1];
                var name = data[2];
                var margin = double.Parse(data[3]);
                var total = int.Parse(data[4]);
                var success = int.Parse(data[5]);
                var avgOdd = double.Parse(data[6]);
                var successRate = double.Parse(data[7]);
                var moneyMade = double.Parse(data[8]);
                var moneyPerGame = double.Parse(data[9]);
                var season = data.Length < 11 ? string.Empty : data[10];

                if (success < 50 ||
                    //successRate < 0.9 ||
                    moneyPerGame <= 0.0)
                    continue;

                var info = new LeagueInfo()
                {
                    Sport = sport,
                    Country = country,
                    Name = name
                };

                var league = new LeagueTypeData(margin, info)
                {
                    TotalRecords = total,
                    SuccessRecords = success,
                    SuccessRate = successRate,
                    MoneyMade = moneyMade,
                    MoneyPerGame = moneyPerGame
                };

                results.Add(league);
            }

            return results.ToArray();
        }
    }
}
