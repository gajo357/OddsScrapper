using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class OddsMatcher
    {
        private const string BetsFolderName = "GamesToBet";
        public void MatchGamesWithArchivedData(string date)
        {
            var games = GetAllTommorowsGames(date);

            var resultsFiles = HelperMethods.GetAnalysedResultsFiles();
            foreach (var file in resultsFiles)
            {
                var archivedData = GetArchivedData(file);
                var fileName = Path.GetFileNameWithoutExtension(file).Replace("results_", string.Empty);
                MatchGames(archivedData, games, date, fileName);
            }

            var dataBySeasonsHome = GetArchivedData(HelperMethods.GetAnalysedResultsFile(1, ResultType.Seasonal));
            var dataAllPositive = GetArchivedData(HelperMethods.GetAnalysedResultsFile(1, ResultType.All, AnalysisType.Positive));
            var positiveData = MatchTwoArchives(dataBySeasonsHome, dataAllPositive);
            MatchGames(positiveData, games, date, "positive_1");

            dataBySeasonsHome = GetArchivedData(HelperMethods.GetAnalysedResultsFile(2, ResultType.Seasonal));
            dataAllPositive = GetArchivedData(HelperMethods.GetAnalysedResultsFile(2, ResultType.All, AnalysisType.Positive));
            positiveData = MatchTwoArchives(dataBySeasonsHome, dataAllPositive);
            MatchGames(positiveData, games, date, "positive_2");

            var allGames1 = GetArchivedData(HelperMethods.GetAnalysedResultsFile(1, ResultType.All));
            MatchGames(allGames1, games, date, "all_1", g => g.Kelly);

            var allGames2 = GetArchivedData(HelperMethods.GetAnalysedResultsFile(1, ResultType.All));
            MatchGames(allGames2, games, date, "all_2", g => g.Kelly);

            WriteMustWinGames(games, date);
        }

        private void WriteMustWinGames(IList<GameInfo> games, string date)
        {
            const string headerLine = "Sport,Country,League,Participants,Odds,Must Win";

            using (var fileStream = File.AppendText(Path.Combine(HelperMethods.GetSolutionDirectory(), BetsFolderName, $"MustWin_{date}.csv")))
            {
                var headerWritten = false;
                foreach (var game in games)
                {
                    var mustWinBets = GetMustWinBets(game.Odds);
                    if (!mustWinBets)
                        continue;

                    if(!headerWritten)
                    {
                        fileStream.WriteLine(headerLine);
                        headerWritten = true;
                    }

                    var line = $"{game.Sport},{game.Country},{game.League},{game.Participants},{String.Join(",", game.Odds.Select(s => s).ToArray())}";
                    fileStream.WriteLine(line);
                }
            }
        }

        private bool GetMustWinBets(double[] odds)
        {
            if(odds.Length == 2)
            {                
                return ((odds[0] - 1.0) * (odds[1] - 1.0)) > 1.0;
            }

            if(odds.Length == 3)
            {
                return ((odds[0] * (odds[1] + odds[2])) / (odds[1]* odds[2]*(odds[0] - 1.0))) < 1.0;
            }

            return false;
        }

        /// <summary>
        /// Get the data that is positive in both All and BySeasons categories
        /// </summary>
        /// <param name="dataBySeasonsHome"></param>
        /// <param name="dataAllPositive"></param>
        /// <returns></returns>
        private LeagueTypeData[] MatchTwoArchives(LeagueTypeData[] dataBySeasonsHome, LeagueTypeData[] dataAllPositive)
        {
            var results = new List<LeagueTypeData>();
            foreach (var allPositiveData in dataAllPositive)
            {
                var bySeasonsData = dataBySeasonsHome.FirstOrDefault(s => s.Info.Sport == allPositiveData.Info.Sport &&
                                                                          s.Info.Country == allPositiveData.Info.Country &&
                                                                          s.Info.Name == allPositiveData.Info.Name &&
                                                                          s.DoesOddBelong(allPositiveData.UpMargin));

                if (bySeasonsData == null)
                    continue;

                if (bySeasonsData.KellyPercentage <= 0)
                    continue;

                results.Add(allPositiveData);
            }

            return results.ToArray();
        }

        private void MatchGames(LeagueTypeData[] archivedData, IEnumerable<GameInfo> games, string date, string fileName, Func<GameInfo, double> orderBy = null)
        {
            if (orderBy == null)
                orderBy = f => f.MoneyPerGame;

            var filteredGames = GetFilteredGames(archivedData, games, fileName);

            using (var fileStream = File.AppendText(Path.Combine(HelperMethods.GetSolutionDirectory(), BetsFolderName, $"GamesToBet_{date}_{fileName}.csv")))
            {
                var headerLine = "Sport,Country,League,Total Records,Success Rate,Money Per Game,Kelly,Participants,Odds";
                fileStream.WriteLine(headerLine);

                foreach (var game in filteredGames.OrderByDescending(orderBy))
                {
                    var line = $"{game.Sport},{game.Country},{game.League},{game.TotalRecords},{game.SuccessRate:F4},{game.MoneyPerGame:F4},{game.Kelly:F4},{game.Participants},{String.Join(",", game.Odds.Select(s => s).ToArray())}";
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
                game.Kelly = HelperMethods.CalculateKellyCriterionPercentage(game.BestOdd, game.SuccessRate);
                
                yield return game;
            }
        }

        private IList<GameInfo> GetAllTommorowsGames(string gameDate)
        {
            var results = new List<GameInfo>();

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

                    results.Add(game);
                }
            }

            return results;
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
