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
        public LeagueInfo Info;

        public LeagueTypeData Le11 { get; } = new LeagueTypeData(1.1);
        public LeagueTypeData Le12 { get; } = new LeagueTypeData(1.2);
        public LeagueTypeData Le13 { get; } = new LeagueTypeData(1.3);
        public LeagueTypeData Le14 { get; } = new LeagueTypeData(1.4);
        public LeagueTypeData Le15 { get; } = new LeagueTypeData(1.5);

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
                ltd.WriteLeagueData(stream, Info);
            }
        }
    }

    public class LeagueTypeData
    {
        public LeagueTypeData(double margin)
        {
            Margin = margin;
        }

        public double Margin { get; }
        public double OddsSum = 0;

        public int TotalRecords = 0;
        public int SuccessRecords = 0;

        public double MoneyMade = 0;

        private double _moneyPerGame = 0;
        public double MoneyPerGame => _moneyPerGame;

        private double _successRate = 0;
        public double SuccessRate => _successRate;

        public void WriteLeagueData(StreamWriter stream, LeagueInfo info)
        {
            if (TotalRecords == 0)
                return;

            var line = $"{info.Sport},{info.Country},{HelperMethods.MakeValidFileName(info.Name)},{Margin:F2},{TotalRecords},{SuccessRecords},{OddsSum / TotalRecords:F4},{SuccessRate:F4},{MoneyMade},{MoneyPerGame:F4}";
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

            _moneyPerGame = MoneyMade / TotalRecords;
            _successRate = (double)SuccessRecords / TotalRecords;
        }
    }
}
