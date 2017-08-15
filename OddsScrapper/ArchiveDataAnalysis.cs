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

                foreach (var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    if (data.Length != 4)
                        continue;

                    var season = data[0];
                    var participants = data[1];
                    var odd = double.Parse(data[2]);
                    var success = int.Parse(data[3]) == 1;
                                                            
                    leagueData.AddData(odd, success, season);
                }

                allLeagues.Add(leagueData);
            }

            using (var stream = File.AppendText(HelperMethods.GetAllAnalysedResultsFile()))
            {
                var line = $"Sport,Country,League Name,Margin,Total Records,Success Records,Avg. Odd,Success Rate,Money Made,Money Per Game,Rate Of Available Money";
                stream.WriteLine(line);
                using (var streamBySeasons = File.AppendText(HelperMethods.GetBySeasonsAnalysedResultsFile()))
                {
                    line = $"Sport,Country,League Name,Margin,Total Records,Records Per Season,Success Records,Avg. Odd,Success Rate,Money Made,Money Per Game,Rate Of Available Money,Number Of Seasons,No Of Positive Seasons,Money Low,Money High,Rate Low,Rate High";
                    streamBySeasons.WriteLine(line);
                    foreach (var league in allLeagues)
                    {
                        league.WriteLeagueData(stream);
                        league.WriteLeagueDataBySeasons(streamBySeasons);
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
