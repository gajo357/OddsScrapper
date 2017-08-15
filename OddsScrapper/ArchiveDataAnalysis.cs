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

            var leaguesDict = new Dictionary<Tuple<string, string, string>, IDictionary<string, LeagueOddsData>>();
            var allLeagues = new List<LeagueOddsData>();
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                var sport = fileName[0];
                var country = fileName[1];
                var leagueName = fileName[2];

                var key = new Tuple<string, string, string>(sport, country, leagueName);
                if (!leaguesInfo.ContainsKey(key))
                    continue;

                var info = leaguesInfo[key];

                var leagueData = new LeagueOddsData(info);

                if (!leaguesDict.ContainsKey(key))
                    leaguesDict.Add(key, new Dictionary<string, LeagueOddsData>());

                foreach (var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    if (data.Length != 4)
                        continue;

                    var season = data[0];
                    var participants = data[1];
                    var odd = double.Parse(data[2]);
                    var success = int.Parse(data[3]) == 1;
                                                            
                    if (!leaguesDict[key].ContainsKey(season))
                    {
                        leaguesDict[key].Add(season, new LeagueOddsData(info));
                    }
                    var leagueDictData = leaguesDict[key][season];

                    leagueDictData.AddData(odd, success);
                    leagueData.AddData(odd, success);
                }

                allLeagues.Add(leagueData);
            }

            using (var stream = File.AppendText(HelperMethods.GetAllAnalysedResultsFile()))
            {
                foreach (var pair in allLeagues)
                {
                    pair.WriteLeagueData(stream);
                }
            }

            foreach(var spair in leaguesDict)
            {
                var sport = spair.Key.Item1;
                var country = spair.Key.Item2;
                var name = spair.Key.Item3;
                using (var stream = File.AppendText(Path.Combine(HelperMethods.GetAnalysedArchivedDataFolderPath(), $"results_{sport}.csv")))
                {
                    // record leagues that have positive in all seasons (in a single category) 
                    // or at least last 5 seasons
                    // or smth

                    foreach (var leaguePair in spair.Value)
                    {
                        var league = leaguePair.Value;
                        var season = leaguePair.Key;

                        league.WriteLeagueData(stream, season);
                    }
                }
            }
        }

        private string[] CupNames = new[] { "cup", "copa", "cupen" };
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
