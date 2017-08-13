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

            var leaguesDict = new Dictionary<string, List<LeagueOddsData>>();
            var allLeagues = new List<LeagueOddsData>();
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                var sport = fileName[0];
                var country = fileName[1];
                var leagueName = fileName[2];
                if (!leaguesInfo.ContainsKey(sport))
                    continue;

                var info = leaguesInfo[sport][country][leagueName];

                var leagueData = new LeagueOddsData() { Info = info };

                foreach(var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    if (data.Length != 3)
                        continue;

                    var participants = data[0];
                    var odd = double.Parse(data[1]);
                    var success = int.Parse(data[2]) == 1;

                    leagueData.AddData(odd, success);
                }

                if (leagueData.Le15.TotalRecords == 0)
                    continue;

                if (!leaguesDict.ContainsKey(sport))
                    leaguesDict.Add(sport, new List<LeagueOddsData>());

                leaguesDict[sport].Add(leagueData);
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
                var sport = spair.Key;
                using (var stream = File.AppendText(Path.Combine(HelperMethods.GetAnalysedArchivedDataFolderPath(), $"results_{sport}.csv")))
                {
                    foreach (var league in spair.Value)
                    {
                        league.WriteLeagueData(stream);
                    }
                }

                using (var stream = File.AppendText(Path.Combine(HelperMethods.GetAnalysedArchivedDataFolderPath(), $"results_{sport}_first_man_league.csv")))
                {
                    foreach (var league in spair.Value.Where(s => s.Info.IsFirst && !s.Info.IsWomen && !s.Info.IsCup))
                    {
                        league.WriteLeagueData(stream);
                    }
                }
            }
        }

        private string[] CupNames = new[] { "cup", "copa", "cupen" };
        private string Women = "women";
        private IDictionary<string, IDictionary<string, IDictionary<string, LeagueInfo>>> ReadLeaguesInfos(IEnumerable<string> leaguesInfoFiles)
        {
            var result = new Dictionary<string, IDictionary<string, IDictionary<string, LeagueInfo>>>();
            foreach(var fileName in leaguesInfoFiles)
            {
                foreach (var line in File.ReadLines(fileName))
                {
                    var data = line.Split(',');
                    var sport = data[0];
                    var country = data[1];
                    var name = data[2];
                    var isFirst = int.Parse(data[3]) == 1 ? true : false;
                    var isCup = int.Parse(data[4]) == 1 || CupNames.Any(name.Contains) ? true : false;
                    var isWomen = int.Parse(data[5]) == 1 || name.Contains(Women) ? true : false;

                    var league = new LeagueInfo();
                    league.Sport = sport;
                    league.Country = country;
                    league.Name = name;
                    league.IsFirst = isFirst;
                    league.IsCup = isCup;
                    league.IsWomen = isWomen;

                    if (!result.ContainsKey(sport))
                        result.Add(sport, new Dictionary<string, IDictionary<string, LeagueInfo>>());
                    
                    if (!result[sport].ContainsKey(country))
                        result[sport].Add(country, new Dictionary<string, LeagueInfo>());

                    result[sport][country].Add(name, league);
                }
            }

            return result;
        }
    }
}
