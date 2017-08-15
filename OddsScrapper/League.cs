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
            return $"{Sport},{Country},{HelperMethods.MakeValidFileName(Name)},{(IsFirst ? 1 : 0)}";
        }
    }

    public class LeagueOddsData
    {
        public LeagueOddsData(LeagueInfo info)
        {
            Info = info;
            Data = new[]
            {
                new LeagueTypeData(1.0, 1.1, info),
                new LeagueTypeData(1.1, 1.2, info),
                new LeagueTypeData(1.2, 1.3, info),
                new LeagueTypeData(1.3, 1.4, info),
                new LeagueTypeData(1.4, 1.5, info)
            };
        }

        public LeagueInfo Info { get; }

        private LeagueTypeData[] Data { get; }

        public void AddData(double odd, bool success)
        {
            foreach (var ltd in Data)
            {
                ltd.AddRecord(odd, success);
            }
        }

        public void WriteLeagueData(StreamWriter stream, string season = null)
        {
            foreach (var ltd in Data)
            {
                ltd.WriteLeagueData(stream, season);
            }
        }
    }

    public class LeagueTypeData
    {
        public LeagueTypeData(double downMargin, double upMargin, LeagueInfo info)
        {
            DownMargin = downMargin;
            UpMargin = upMargin;
            Info = info;
        }

        public LeagueInfo Info { get; }
        
        public double DownMargin { get; }
        public double UpMargin { get; }
        public double OddsSum = 0;

        public int TotalRecords = 0;
        public int SuccessRecords = 0;

        public double MoneyMade = 0;

        public double MoneyPerGame = 0;

        public double SuccessRate = 0;

        public double RateOfAvailableMoney = 0;

        public double AvgOdd = 0;

        public void WriteLeagueData(StreamWriter stream, string season)
        {
            if (TotalRecords == 0)
                return;

            var line = $"{Info.Sport},{Info.Country},{HelperMethods.MakeValidFileName(Info.Name)},{UpMargin:F2},{TotalRecords},{SuccessRecords},{AvgOdd:F4},{SuccessRate:F4},{MoneyMade},{MoneyPerGame:F4},{RateOfAvailableMoney:F4},{season}";
            stream.WriteLine(line);
        }

        public bool DoesOddBelong(double odd)
        {
            return odd <= UpMargin && odd > DownMargin;
        }

        public void AddRecord(double odd, bool success)
        {
            if (!DoesOddBelong(odd))
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

            AvgOdd = OddsSum / TotalRecords;
            RateOfAvailableMoney = MoneyMade / (OddsSum - TotalRecords);
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
