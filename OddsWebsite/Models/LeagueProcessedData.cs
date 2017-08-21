using System.Collections.Generic;
using OddsWebsite.Helpers;

namespace OddsWebsite.Models
{
    public class LeagueProcessedData
    {
        private IDictionary<int, LeagueAnalysisInfo> DataBySeasons { get; } = new Dictionary<int, LeagueAnalysisInfo>();
        
        public List<LeagueAnalysisInfo> SeasonsData { get; } = new List<LeagueAnalysisInfo>();

        public LeagueAnalysisInfo SeasonsSummary { get; } = new LeagueAnalysisInfo();

        public LeagueAnalysisInfo FullSummary { get; } = new LeagueAnalysisInfo();
        
        public void AddGames(IEnumerable<Game> games)
        {
            foreach (var game in games)
            {
                AddGame(game);
            }
        }

        private void AddGame(Game game)
        {
            var season = game.Season;

            FullSummary.AddGame(game);

            if (!DataBySeasons.ContainsKey(season))
            {
                DataBySeasons.Add(season, new LeagueAnalysisInfo());
                SeasonsData.Add(DataBySeasons[season]);
            }

            DataBySeasons[season].AddGame(game);

            CalculateSeasonsSummary();
        }

        private void CalculateSeasonsSummary()
        {
            if (FullSummary.TotalRecords == 0)
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
            }

            avgOdd /= numberOfSeasons;
            moneyPerGame /= numberOfSeasons;
            successRate /= numberOfSeasons;
            var kelly = CalculationHelper.CalculateKellyCriterionPercentage(avgOdd, successRate);

            SeasonsSummary.KellyPercentage = kelly;
            SeasonsSummary.AvgOdd = avgOdd;
            SeasonsSummary.MoneyPerGame = moneyPerGame;
            SeasonsSummary.SuccessRate = successRate;
            SeasonsSummary.TotalRecords = FullSummary.TotalRecords;
            SeasonsSummary.SuccessRecords = FullSummary.SuccessRecords;
        }
    }
}
