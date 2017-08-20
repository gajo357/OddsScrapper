using OddsWebsite.Helpers;
using System.Collections.Generic;

namespace OddsWebsite.Models
{
    public class LeagueProcessedData
    {
        public LeagueProcessedData(double downMargin, double upMargin)
        {
            DownMargin = downMargin;
            UpMargin = upMargin;

            SummaryBySeasons = new LeagueProcessedData(downMargin, upMargin);
        }

        private IDictionary<int, LeagueProcessedData> DataBySeasons { get; }

        public LeagueProcessedData SummaryBySeasons { get; }

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

        public void AddGame(Game game, bool saveBySeasons = true)
        {
            TotalRecords++;

            var success = game.Bet == game.Winner;
            var odd = game.WinningOdd;
            var season = game.Season;

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
            KellyPercentage = CalculationHelper.CalculateKellyCriterionPercentage(AvgOdd, SuccessRate);

            if (!DataBySeasons.ContainsKey(season))
                DataBySeasons.Add(season, new LeagueProcessedData(DownMargin, UpMargin));

            DataBySeasons[season].AddGame(game, false);
        }
    }
}
