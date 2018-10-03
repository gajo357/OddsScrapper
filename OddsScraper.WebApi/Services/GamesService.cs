using System;
using System.Collections.Generic;
using System.Linq;
using OddsScraper.WebApi.Models;

namespace OddsScraper.WebApi.Services
{
    public class GamesService : IGamesService
    {
        private IHashService UserLoginService { get; }
        private static GameDto[] EmptyGames { get; } = new GameDto[0];

        public GamesService(IUserLoginService userLoginService) 
        {
            UserLoginService = userLoginService;
        }

        public IEnumerable<GameDto> GetDaysGamesInfo(string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return EmptyGames;

            return FSharp.Scraping.FutureGamesDownload.downloadAllDayGameInfos(DateTime.Now).Select(GameDto.Create);
        }

        public IEnumerable<GameDto> GetGameInfos(double timeSpan, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return EmptyGames;

            return FSharp.Scraping.FutureGamesDownload.downloadGameInfos(DateTime.Now, timeSpan).Select(GameDto.Create);
        }

        public IEnumerable<GameDto> GetGames(double timeSpan, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return EmptyGames;

            return FSharp.Scraping.FutureGamesDownload.downloadGames(DateTime.Now, timeSpan).Select(GameDto.Create);
        }

        public GameDto GetGame(string gameLink, string user)
        {
            if (!UserLoginService.IsHashPresent(user))
                return null;

            return GameDto.Create(FSharp.Scraping.FutureGamesDownload.readGameFromLink(gameLink));
        }
    }
}
