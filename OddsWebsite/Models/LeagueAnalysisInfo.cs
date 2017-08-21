using OddsWebsite.Helpers;

namespace OddsWebsite.Models
{
    public class LeagueAnalysisInfo
    {
        private double _oddsSum;
        private double _moneyMade;

        public int TotalRecords;
        public int SuccessRecords;
        public double KellyPercentage;
        public double MoneyPerGame;
        public double SuccessRate;
        public double AvgOdd;

        public void AddGame(Game game)
        {
            TotalRecords++;

            var success = game.Bet == game.Winner;
            var odd = game.WinningOdd;

            if (success)
            {
                SuccessRecords++;
                _moneyMade += odd - 1;
            }
            else
            {
                _moneyMade -= 1;
            }
            _oddsSum += odd;

            AvgOdd = _oddsSum / TotalRecords;
            MoneyPerGame = _moneyMade / TotalRecords;
            SuccessRate = (double)SuccessRecords / TotalRecords;
            KellyPercentage = CalculationHelper.CalculateKellyCriterionPercentage(AvgOdd, SuccessRate);
        }
    }
}
