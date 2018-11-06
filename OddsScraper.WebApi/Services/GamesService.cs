using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OddsScraper.WebApi.Models;

namespace OddsScraper.WebApi.Services
{
    public class GamesService : IGamesService
    {
        private IHashService UserLoginService { get; }
        private FSharp.CommonScraping.Downloader.IDownloader Downloader { get; }

        public GamesService(IUserLoginService userLoginService, FSharp.CommonScraping.Downloader.IDownloader downloader) 
        {
            UserLoginService = userLoginService;
            Downloader = downloader;
        }

        public async Task<GameDto[]> GetGameInfosAsync(int? gamesCount, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            var games = await GetGameInfoModelsAsync(gamesCount);
            return games.Select(GameDto.Create).ToArray();
        }

        private async Task<IEnumerable<FSharp.CommonScraping.Models.Game>> GetGameInfoModelsAsync(int? gamesCount)
        {
            if (!gamesCount.HasValue || gamesCount <= 0)
            {
                return await Downloader.DownloadGameInfos(DateTime.Now);
            }

            if (gamesCount > 100)
            {
                var games = await Downloader.DownloadGameInfos(DateTime.Now);
                return games.Take(gamesCount.Value);
            }

            var widgetGames = await Downloader.DownloadFromWidget();
            return widgetGames.Take(gamesCount.Value);
        }

        public GameDto[] GetGames(double timeSpan, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            return FSharp.CommonScraping.FutureGamesDownload.downloadGames(DateTime.Now).Select(GameDto.Create).ToArray();
        }

        public async Task<GameDto> GetGameAsync(string gameLink, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            return GameDto.Create(await Downloader.ReadGameFromLink(gameLink));
        }
    }
}
