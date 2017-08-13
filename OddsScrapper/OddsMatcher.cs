using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddsScrapper
{
    public class OddsMatcher
    {
        public void MatchGamesWithArchivedData()
        {
            var archivedData = GetArchivedData();

            var games = GetAllTommorowsGames();

            MatchGames(archivedData, games);
        }

        private void MatchGames(LeagueTypeData[] archivedData, IList<GameInfo> games)
        {
            using (var fileStream = File.AppendText("GamesToBet.csv"))
            {
                var filteredGames = new List<GameInfo>();
                foreach (var game in games)
                {
                    var league = archivedData.FirstOrDefault(s => s.Info.Sport == game.Sport && 
                                                                s.Info.Country == game.Country && 
                                                                s.Info.Name == game.League);
                    if (league == null)
                        continue;

                    game.MoneyPerGame = league.MoneyPerGame;
                    game.TotalRecords = league.TotalRecords;
                    game.SuccessRate = league.SuccessRate;

                    filteredGames.Add(game);
                }

                foreach(var game in filteredGames.OrderByDescending(s => s.MoneyPerGame))
                {
                    var line = $"{game.Sport},{game.Country},{game.League},{game.TotalRecords},{game.SuccessRate:F4},{game.MoneyPerGame:F4},{game.Participants},{String.Join(",", game.Odds.Select(s => s).ToArray())}";
                    fileStream.WriteLine(line);
                }
            }
        }

        private IList<GameInfo> GetAllTommorowsGames()
        {
            var results = new List<GameInfo>();

            var dataDirectory = HelperMethods.GetTommorowsGamesFolderPath();
            var files = Directory.GetFiles(dataDirectory, "*.csv");
            foreach(var file in files)
            {
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

                    results.Add(game);
                }
            }

            return results;
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

                if (success < 50 ||
                    successRate < 0.90 ||
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
