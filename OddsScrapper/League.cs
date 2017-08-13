using System;
using System.Collections.Generic;
using System.IO;

namespace OddsScrapper
{
    public class LeagueInfo
    {
        public bool IsFirst;
        public string Country;
        public string Name;

        public string Link;
        internal bool IsCup;
        internal bool IsWomen;
        internal string Sport;

        public override string ToString()
        {
            return $"{Sport},{Country},{HelperMethods.MakeValidFileName(Name)},{(IsFirst ? 1 : 0)},{(IsCup ? 1 : 0)},{(IsWomen ? 1 : 0)}";
        }
    }

    public class LeagueOddsData
    {
        public LeagueOddsData(LeagueInfo info)
        {
            Info = info;
            Le11 = new LeagueTypeData(1.1, info);
            Le12 = new LeagueTypeData(1.2, info);
            Le13 = new LeagueTypeData(1.3, info);
            Le14 = new LeagueTypeData(1.4, info);
            Le15 = new LeagueTypeData(1.5, info);
        }

        public LeagueInfo Info { get; }

        public LeagueTypeData Le11 { get; }
        public LeagueTypeData Le12 { get; }
        public LeagueTypeData Le13 { get; }
        public LeagueTypeData Le14 { get; }
        public LeagueTypeData Le15 { get; }

        private IEnumerable<LeagueTypeData> Data => new[] { Le11, Le12, Le13, Le14, Le15 };

        public void AddData(double odd, bool success)
        {
            foreach (var ltd in Data)
            {
                ltd.AddRecord(odd, success);
            }
        }

        public void WriteLeagueData(StreamWriter stream)
        {
            foreach (var ltd in Data)
            {
                ltd.WriteLeagueData(stream);
            }
        }
    }

    public class LeagueTypeData
    {
        public LeagueTypeData(double margin, LeagueInfo info)
        {
            Margin = margin;
            Info = info;
        }

        public LeagueInfo Info { get; }
        
        public double Margin { get; }
        public double OddsSum = 0;

        public int TotalRecords = 0;
        public int SuccessRecords = 0;

        public double MoneyMade = 0;

        public double MoneyPerGame = 0;

        public double SuccessRate = 0;

        public void WriteLeagueData(StreamWriter stream)
        {
            if (TotalRecords == 0)
                return;

            var line = $"{Info.Sport},{Info.Country},{HelperMethods.MakeValidFileName(Info.Name)},{Margin:F2},{TotalRecords},{SuccessRecords},{OddsSum / TotalRecords:F4},{SuccessRate:F4},{MoneyMade},{MoneyPerGame:F4}";
            stream.WriteLine(line);
        }

        public void AddRecord(double odd, bool success)
        {
            if (odd > Margin)
                return;

            TotalRecords++;
            if (success)
            {
                SuccessRecords++;
                MoneyMade += odd - 1;
            }
            else
            {
                MoneyMade -= 1;
            }
            OddsSum += odd;

            MoneyPerGame = MoneyMade / TotalRecords;
            SuccessRate = (double)SuccessRecords / TotalRecords;
        }
    }

    public class GameInfo
    {
        public string Country;
        public string League;
        public string Sport;

        public int TotalRecords = 0;
        public double MoneyPerGame = 0;
        public double SuccessRate = 0;

        public string Participants;

        public double[] Odds;
    }
}
