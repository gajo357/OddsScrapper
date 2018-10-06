using System;
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

        public async Task<GameDto[]> GetDaysGamesInfoAsync(string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            var games = await Downloader.DownloadAllDayGameInfos(DateTime.Now);
            return games.Select(GameDto.Create).ToArray();
        }

        public async Task<GameDto[]> GetGameInfosAsync(double timeSpan, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            var games = await Downloader.DownloadGameInfos(DateTime.Now, timeSpan);
            return games.Select(GameDto.Create).ToArray();
        }

        public GameDto[] GetGames(double timeSpan, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            return FSharp.CommonScraping.FutureGamesDownload.downloadGames(DateTime.Now, timeSpan).Select(GameDto.Create).ToArray();
        }

        public async Task<GameDto> GetGameAsync(string gameLink, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            return GameDto.Create(await Downloader.ReadGameFromLink(gameLink));
        }
    }
}
