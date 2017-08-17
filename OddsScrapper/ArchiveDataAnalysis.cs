using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class ArchiveDataAnalysis
    {
        public void Analyse()
        {
            var dataDirectory = HelperMethods.GetArchiveFolderPath();
            var files = Directory.GetFiles(dataDirectory, "*.csv");
            var leaguesInfoFiles = Directory.GetFiles(dataDirectory, "*.txt");
            var leaguesInfo = ReadLeaguesInfos(leaguesInfoFiles);
            
            var allLeagues = CollectLeaguesData(files, leaguesInfo);

            WriteLeaguesToFilesUnfiltered(allLeagues);
            WriteLeaguesToFilesFiltered(allLeagues);
        }

        private void WriteLeaguesToFilesFiltered(Dictionary<int, IList<LeagueOddsData>> allLeagues)
        {
            WriteAllLeaguesResultsToFilesFiltered(allLeagues);
            WriteLeaguesBySeasonsResultsToFilesFiltered(allLeagues);
        }

        private void WriteLeaguesBySeasonsResultsToFilesFiltered(Dictionary<int, IList<LeagueOddsData>> allLeagues)
        {
            foreach (var data in allLeagues)
            {
                var bet = data.Key;

                using (var streamNegative = File.AppendText(HelperMethods.GetAnalysedResultsFile(bet, ResultType.Seasonal, AnalysisType.Negative)))
                {
                    streamNegative.WriteLine(ResultsBySeasonsHeader);

                    using (var streamPositive = File.AppendText(HelperMethods.GetAnalysedResultsFile(bet, ResultType.Seasonal, AnalysisType.Positive)))
                    {
                        streamPositive.WriteLine(ResultsBySeasonsHeader);

                        foreach (var league in data.Value)
                        {
                            foreach (var ltd in league.Data)
                            {
                                if (ltd.TotalRecords < 20)
                                    continue;

                                ltd.WriteLeagueDataBySeasons(streamNegative, checkAllNegative: true);
                                ltd.WriteLeagueDataBySeasons(streamPositive, checkAllPositive: true);
                            }
                        }
                    }
                }
            }
        }

        private void WriteAllLeaguesResultsToFilesFiltered(Dictionary<int, IList<LeagueOddsData>> allLeagues)
        {
            foreach (var data in allLeagues)
            {
                var bet = data.Key;

                using (var streamPositive = File.AppendText(HelperMethods.GetAnalysedResultsFile(bet, ResultType.All, AnalysisType.Positive)))
                {
                    streamPositive.WriteLine(AllResultsHeader);
                    using (var streamNegative = File.AppendText(HelperMethods.GetAnalysedResultsFile(bet, ResultType.All, AnalysisType.Negative)))
                    {
                        streamNegative.WriteLine(AllResultsHeader);

                        foreach (var league in data.Value)
                        {
                            foreach(var ltd in league.Data)
                            {
                                if (ltd.TotalRecords < 20)
                                    continue;

                                if(ltd.KellyPercentage < 0)
                                    ltd.WriteLeagueData(streamNegative);

                                if (ltd.KellyPercentage > 0)
                                    ltd.WriteLeagueData(streamPositive);
                            }
                        }
                    }
                }
            }
        }

        private const string AllResultsHeader = "Sport,Country,League Name,Margin,Total Records,Avg. Odd,Success Rate,Money Per Game,Kelly";
        private const string ResultsBySeasonsHeader = "Sport,Country,League Name,Margin,Total Records,Avg. Odd,Success Rate,Money Per Game,Kelly,Number Of Seasons,No Of Positive Seasons,Money Low,Money High";
        private void WriteLeaguesToFilesUnfiltered(Dictionary<int, IList<LeagueOddsData>> allLeagues)
        {
            foreach (var data in allLeagues)
            {
                var bet = data.Key;
                using (var stream = File.AppendText(HelperMethods.GetAnalysedResultsFile(bet, ResultType.All)))
                {
                    stream.WriteLine(AllResultsHeader);
                    using (var streamBySeasons = File.AppendText(HelperMethods.GetAnalysedResultsFile(bet, ResultType.Seasonal)))
                    {
                        streamBySeasons.WriteLine(ResultsBySeasonsHeader);

                        foreach (var league in data.Value)
                        {
                            league.WriteLeagueData(stream);
                            league.WriteLeagueDataBySeasons(streamBySeasons);
                        }
                    }
                }
            }
        }

        private Dictionary<int, IList<LeagueOddsData>> CollectLeaguesData(string[] files, IDictionary<Tuple<string, string, string>, LeagueInfo> leaguesInfo)
        {
            var allLeagues = new Dictionary<int, IList<LeagueOddsData>>()
            {
                { 1, new List<LeagueOddsData>() },
                { 2, new List<LeagueOddsData>() }
            };

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                var sport = fileName[0];
                var country = fileName[1];
                var leagueName = fileName[2];

                var key = new Tuple<string, string, string>(sport, country, leagueName);
                if (!leaguesInfo.ContainsKey(key))
                    continue;

                var info = leaguesInfo[key];

                var homeLeagueData = new LeagueOddsData(info);
                var awayLeagueData = new LeagueOddsData(info);

                foreach (var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    if (data.Length != 5)
                        continue;

                    var season = data[0];
                    if (season == "Season")
                        continue;

                    var participants = data[1];
                    var odd = double.Parse(data[2]);
                    var bestBet = int.Parse(data[3]);
                    var winBet = int.Parse(data[4]);
                    var success = bestBet == winBet;

                    if (bestBet == 1)
                        homeLeagueData.AddData(odd, success, season);
                    else if (bestBet == 2)
                        awayLeagueData.AddData(odd, success, season);
                }

                allLeagues[1].Add(homeLeagueData);
                allLeagues[2].Add(awayLeagueData);
            }

            return allLeagues;
        }

        private string[] CupNames = new[] { "cup", "copa", "cupen", "coupe", "coppa" };
        private string Women = "women";
        private IDictionary<Tuple<string, string, string>, LeagueInfo> ReadLeaguesInfos(IEnumerable<string> leaguesInfoFiles)
        {
            var result = new Dictionary<Tuple<string, string, string>, LeagueInfo>();
            foreach(var fileName in leaguesInfoFiles)
            {
                foreach (var line in File.ReadLines(fileName))
                {
                    var data = line.Split(',');
                    var sport = data[0];
                    if (sport == "Sport")
                        continue;

                    var country = data[1];
                    var name = data[2];
                    var isFirst = int.Parse(data[3]) == 1;
                    var isCup = CupNames.Any(name.Contains);
                    var isWomen = name.Contains(Women);

                    var league = new LeagueInfo();
                    league.Sport = sport;
                    league.Country = country;
                    league.Name = name;
                    league.IsFirst = isFirst;
                    league.IsCup = isCup;
                    league.IsWomen = isWomen;

                    var key = new Tuple<string, string, string>(sport, country, name);
                    if (!result.ContainsKey(key))
                        result.Add(key, league);
                }
            }

            return result;
        }
    }
}
