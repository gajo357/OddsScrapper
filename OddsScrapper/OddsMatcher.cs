﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class OddsMatcher
    {
        public void MatchGamesWithArchivedData(string date)
        {
            var games = GetAllTommorowsGames(date);

            var results_files = HelperMethods.GetAnalysedResultsFiles();
            foreach(var file in results_files)
            {
                var archivedData = GetArchivedData(file);
                var fileName = Path.GetFileNameWithoutExtension(file).Replace("results_", string.Empty);
                MatchGames(archivedData, games, date, fileName);
            }
        }

        private void MatchGames(LeagueTypeData[] archivedData, IEnumerable<GameInfo> games, string date, string fileName)
        {
            var filteredGames = GetFilteredGames(archivedData, games, fileName);

            using (var fileStream = File.AppendText($"GamesToBet_{date}_{fileName}.csv"))
            {
                var headerLine = "Sport,Country,League,Total Records,Success Rate,Money Per Game,Kelly,Participants,Odds";
                fileStream.WriteLine(headerLine);

                foreach (var game in filteredGames.OrderByDescending(s => s.MoneyPerGame))
                {
                    var kelly = HelperMethods.CalculateKellyCriterionPercentage(game.BestOdd, game.SuccessRate);
                    var line = $"{game.Sport},{game.Country},{game.League},{game.TotalRecords},{game.SuccessRate:F4},{game.MoneyPerGame:F4},{kelly:F4},{game.Participants},{String.Join(",", game.Odds.Select(s => s).ToArray())}";
                    fileStream.WriteLine(line);
                }
            }
        }

        private IEnumerable<GameInfo> GetFilteredGames(LeagueTypeData[] archivedData, IEnumerable<GameInfo> games, string fileName)
        {
            var bet = int.Parse(fileName.Last().ToString());

            foreach (var game in games)
            {
                var bestBet = -1;
                var bestOdd = double.MaxValue;
                
                for (var i = 0; i < game.Odds.Length; i++)
                {
                    if(game.Odds[i] < bestOdd)
                    {
                        bestBet = HelperMethods.GetBetComboFromIndex(game.Odds.Length, i);
                        bestOdd = game.Odds[i];
                    }
                }
                if (bet != bestBet)
                    continue;

                LeagueTypeData league = archivedData.FirstOrDefault(s => s.Info.Sport == game.Sport &&
                                                                         s.Info.Country == game.Country &&
                                                                         s.Info.Name == game.League &&
                                                                         s.DoesOddBelong(bestOdd));

                if (league == null)
                    continue;

                game.MoneyPerGame = league.MoneyPerGame;
                game.TotalRecords = league.TotalRecords;
                game.SuccessRate = league.SuccessRate;
                game.BestOdd = bestOdd;

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
                    if (sport == "Sport")
                        continue;

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

        private LeagueTypeData[] GetArchivedData(string file)
        {
            var results = new List<LeagueTypeData>();
            foreach(var line in File.ReadLines(file))
            {
                var data = line.Split(',');
                var i = 0;
                var sport = data[i++];
                var country = data[i++];
                var name = data[i++];
                double margin;
                if (!double.TryParse(data[i++], out margin))
                    continue;
                var total = double.Parse(data[i++]);
                var avgOdd = double.Parse(data[i++]);
                var successRate = double.Parse(data[i++]);
                var moneyPerGame = double.Parse(data[i++]);
                var kelly = double.Parse(data[i++]);

                //if (success < 50 ||
                //    //successRate < 0.9 ||
                //    moneyPerGame <= 0.0)
                //    continue;

                var info = new LeagueInfo()
                {
                    Sport = sport,
                    Country = country,
                    Name = name
                };

                var league = new LeagueTypeData(margin - 0.1, margin, info)
                {
                    TotalRecords = (int)total,
                    SuccessRate = successRate,
                    KellyPercentage = kelly,
                    MoneyPerGame = moneyPerGame
                };

                results.Add(league);
            }

            return results.ToArray();
        }
    }
}
