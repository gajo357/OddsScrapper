using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OddsScraper.FSharp.CommonScraping;

namespace OddsScraper.Calculator
{
    public static class DownloadHelper
    {
        private static Downloader.IDownloader Downloader { get; } = new Downloader.Downloader();

        public static async Task<bool> LogIn(string username, string password) 
            => await Downloader.LogIn(username, password);

        public async static Task<IEnumerable<GameViewModel>> GetGames(int? gameCount)
        {
            //var games = await Downloader.DownloadGameInfos(DateTime.Now);
            var games = await Downloader.DownloadFromWidget();

            if (gameCount.HasValue)
                games = games.Take(gameCount.Value);

            return games.Select(GameViewModel.Create);
        }

        public static double CalculateAmount(double margin, double balance, double meanOdd, double bookerOdd)
            => FSharp.Common.BettingCalculations.getAmountToBet(margin, balance, meanOdd, bookerOdd);

        public static async Task<Models.Game> GetGame(string link)
            => await Downloader.ReadGameFromLink(link);

        public static async Task RefreashData(GameViewModel viewModel)
        {
            var model = await GetGame(viewModel.GameLink);

            viewModel.HomeMeanOdd = model.MeanOdds.Home;
            viewModel.DrawMeanOdd = model.MeanOdds.Draw;
            viewModel.AwayMeanOdd = model.MeanOdds.Away;

            viewModel.HomeOdd = model.Odds.Home;
            viewModel.DrawOdd = model.Odds.Draw;
            viewModel.AwayOdd = model.Odds.Away;
        }
    }
}
