using System;
using System.Collections.Generic;
using System.Linq;

namespace OddsScraper.Calculator
{
    public static class DownloadHelper
    {
        public static void LogIn(string username, string password) 
            => FSharp.Scraping.CanopyExtensions.loginToOddsPortalWithData(username, password);

        public static IEnumerable<GameViewModel> GetGames(double timeSpan)
            => FSharp.Scraping.FutureGamesDownload
                .downloadGames(DateTime.Now, timeSpan)
                .Select(GameViewModel.Create);

        public static double CalculateAmount(double margin, double balance, double meanOdd, double bookerOdd)
            => FSharp.Common.BettingCalculations.getAmountToBet(margin, balance, meanOdd, bookerOdd);

        public static FSharp.Scraping.FutureGamesDownload.Game GetGame(string link)
            => FSharp.Scraping.FutureGamesDownload.readGameFromLink(link);

        public static void RefreashData(GameViewModel viewModel)
        {
            var model = GetGame(viewModel.GameLink);

            viewModel.HomeMeanOdd = model.HomeMeanOdd;
            viewModel.DrawMeanOdd = model.DrawMeanOdd;
            viewModel.AwayMeanOdd = model.AwayMeanOdd;

            viewModel.HomeOdd = model.HomeOdd;
            viewModel.DrawOdd = model.DrawOdd;
            viewModel.AwayOdd = model.AwayOdd;
        }
    }
}
