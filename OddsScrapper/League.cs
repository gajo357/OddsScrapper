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
                new LeagueTypeData(1.0, 1.3, info),
                new LeagueTypeData(1.3, 1.5, info)
            };
        }

        public LeagueInfo Info { get; }

        public LeagueTypeData[] Data { get; }


        public void AddData(double odd, bool success, string season)
        {
            foreach (var ltd in Data)
            {
                ltd.AddRecord(odd, success, season);
            }
        }

        public void WriteLeagueData(StreamWriter stream)
        {
            foreach (var ltd in Data)
            {
                ltd.WriteLeagueData(stream);
            }
        }

        public void WriteLeagueDataBySeasons(StreamWriter stream)
        {
            foreach (var ltd in Data)
            {
                ltd.WriteLeagueDataBySeasons(stream);
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

            DataBySeasons = new Dictionary<string, LeagueTypeData>();
        }

        public LeagueInfo Info { get; }

        private IDictionary<string, LeagueTypeData> DataBySeasons { get; }

        public double DownMargin { get; }
        public double UpMargin { get; }

        public double KellyPercentage;

        public double OddsSum = 0;

        public int TotalRecords = 0;
        public int SuccessRecords = 0;

        public double MoneyMade = 0;

        public double MoneyPerGame = 0;

        public double SuccessRate = 0;

        public double AvgOdd = 0;

        public void WriteLeagueData(StreamWriter stream)
        {
            if (TotalRecords == 0)
                return;

            var line = $"{Info.Sport},{Info.Country},{HelperMethods.MakeValidFileName(Info.Name)},{UpMargin:F2},{TotalRecords},{AvgOdd:F4},{SuccessRate:F4},{MoneyPerGame:F4},{KellyPercentage:F4}";
            stream.WriteLine(line);
        }

        public void WriteLeagueDataBySeasons(StreamWriter stream, bool checkAllPositive = false, bool checkAllNegative = false)
        {
            if (TotalRecords == 0)
                return;

            var numberOfSeasons = DataBySeasons.Count;
            var avgOdd = 0.0;
            var moneyPerGame = 0.0;
            var successRate = 0.0;

            var numOfPositiveSeasons = 0;
            var moneyHigh = double.MinValue;
            var moneyLow = double.MaxValue;
            foreach (var data in DataBySeasons)
            {
                avgOdd += data.Value.AvgOdd;
                moneyPerGame += data.Value.MoneyPerGame;
                successRate += data.Value.SuccessRate;

                if (data.Value.MoneyPerGame > moneyHigh)
                    moneyHigh = data.Value.MoneyPerGame;
                if (data.Value.MoneyPerGame < moneyLow)
                    moneyLow = data.Value.MoneyPerGame;

                if (data.Value.MoneyPerGame > 0)
                    numOfPositiveSeasons++;
                
                if (checkAllNegative && data.Value.KellyPercentage > 0)
                    return;
                if (checkAllPositive && data.Value.KellyPercentage <= 0)
                    return;
            }

            avgOdd /= numberOfSeasons;
            moneyPerGame /= numberOfSeasons;
            successRate /= numberOfSeasons;
            var kelly = HelperMethods.CalculateKellyCriterionPercentage(avgOdd, successRate);

            var line = $"{Info.Sport},{Info.Country},{HelperMethods.MakeValidFileName(Info.Name)},{UpMargin:F2},{TotalRecords},{avgOdd:F4},{successRate:F4},{moneyPerGame:F4},{kelly:F4},{numberOfSeasons},{numOfPositiveSeasons},{moneyLow},{moneyHigh}";
            stream.WriteLine(line);
        }
        
        public bool DoesOddBelong(double odd)
        {
            return odd <= UpMargin && odd > DownMargin;
        }

        public void AddRecord(double odd, bool success, string season = null)
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
            MoneyPerGame = MoneyMade / TotalRecords;
            SuccessRate = (double)SuccessRecords / TotalRecords;
            KellyPercentage = HelperMethods.CalculateKellyCriterionPercentage(AvgOdd, SuccessRate);

            if (string.IsNullOrEmpty(season))
                return;

            if (!DataBySeasons.ContainsKey(season))
                DataBySeasons.Add(season, new LeagueTypeData(DownMargin, UpMargin, Info));

            DataBySeasons[season].AddRecord(odd, success);
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

        public double BestOdd;

        public int Bet;

        public double Kelly;
    }
}
