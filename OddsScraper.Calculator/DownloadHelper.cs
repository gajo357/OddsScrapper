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

        public static async Task<FutureGamesDownload.Game> GetGame(string link)
            => await Downloader.ReadGameFromLink(link);

        public static async Task RefreashData(GameViewModel viewModel)
        {
            var model = await GetGame(viewModel.GameLink);

            viewModel.HomeMeanOdd = model.HomeMeanOdd;
            viewModel.DrawMeanOdd = model.DrawMeanOdd;
            viewModel.AwayMeanOdd = model.AwayMeanOdd;

            viewModel.HomeOdd = model.HomeOdd;
            viewModel.DrawOdd = model.DrawOdd;
            viewModel.AwayOdd = model.AwayOdd;
        }
    }
}
