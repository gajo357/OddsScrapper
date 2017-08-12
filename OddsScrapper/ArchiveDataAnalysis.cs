using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class ArchiveDataAnalysis
    {
        public void Analyse()
        {
            var dataDirectory = @"C:\Users\Gajo\Documents\Visual Studio 2017\Projects\OddsScrapper\OddsScrapper\bin\Debug";
            var files = Directory.GetFiles(dataDirectory, "*.csv");
            var leaguesInfoFiles = Directory.GetFiles(dataDirectory, "*.txt").Where(s => s.StartsWith("leagues_"));


            var leaguesDict = new Dictionary<string, LeagueData>();
            var total = new LeagueData() { Type = "ZZZ", Sport= "ZZZ", Country = "ZZZ" };
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file).Split('_');
                var sport = fileName[0];
                var country = fileName[1];
                var leagueName = fileName[2];
                
                var leagueData = new LeagueData() { Type = leagueName, Sport = sport, Country = country};
                leaguesDict.Add(file, leagueData);
                foreach(var line in File.ReadLines(file))
                {
                    var data = line.Split(',');
                    if (data.Length != 3)
                        continue;

                    var participants = data[0];
                    var odd = double.Parse(data[1]);
                    var success = int.Parse(data[2]) == 1;

                    leagueData.AddData(odd, success);
                    total.AddData(odd, success);
                }
            }

            using (var stream = File.AppendText("results.csv"))
            {
                foreach (var pair in leaguesDict)
                {
                    pair.Value.WriteLeagueData(stream);
                }

                total.WriteLeagueData(stream);
            }
        }

        private class LeagueData
        {
            public string Sport;
            public string Country;
            public string Type;

            public LeagueTypeData Le11 { get; } = new LeagueTypeData("LE 11");
            public LeagueTypeData Le12 { get; } = new LeagueTypeData("LE 12");
            public LeagueTypeData Le13 { get; } = new LeagueTypeData("LE 13");
            public LeagueTypeData Le14 { get; } = new LeagueTypeData("LE 14");
            public LeagueTypeData Le15 { get; } = new LeagueTypeData("LE 15");

            private IEnumerable<LeagueTypeData> data => new []{ Le11, Le12, Le13, Le14, Le15 };

            public void AddData(double odd, bool success)
            {
                var data = new List<LeagueTypeData>();
                if (odd <= 1.1)
                {
                    data.Add(Le11);
                    data.Add(Le12);
                    data.Add(Le13);
                    data.Add(Le14);
                    data.Add(Le15);
                }
                else if (odd <= 1.2)
                {
                    data.Add(Le12);
                    data.Add(Le13);
                    data.Add(Le14);
                    data.Add(Le15);
                }
                else if (odd <= 1.3)
                {
                    data.Add(Le13);
                    data.Add(Le14);
                    data.Add(Le15);
                }
                else if (odd <= 1.4)
                {
                    data.Add(Le14);
                    data.Add(Le15);
                }
                else
                {
                    data.Add(Le15);
                }

                foreach(var ltd in data)
                {
                    ltd.TotalRecords++;
                    if (success)
                    {
                        ltd.SuccessRecords++;
                        ltd.MoneyMade += odd - 1;
                    }
                    else
                    {
                        ltd.MoneyMade -= 1;
                    }
                    ltd.OddsSum += odd;
                }
            }

            public void WriteLeagueData(StreamWriter stream)
            {
                foreach(var ltd in data)
                {
                    ltd.WriteLeagueData(stream, Type, Sport, Country);
                }
            }
        }

        private class LeagueTypeData
        {
            public LeagueTypeData(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public double OddsSum = 0;

            public int TotalRecords = 0;
            public int SuccessRecords = 0;

            public double MoneyMade = 0;

            public void WriteLeagueData(StreamWriter stream, string leagueName, string sport, string country)
            {
                if (TotalRecords == 0)
                    return;

                var line = $"{sport},{country},{leagueName},{Name},{TotalRecords},{SuccessRecords},{OddsSum / TotalRecords:F4},{(double)SuccessRecords / (double)TotalRecords:F4},{MoneyMade},{MoneyMade/TotalRecords:F4}";
                stream.WriteLine(line);                
            }
        }
    }
}
