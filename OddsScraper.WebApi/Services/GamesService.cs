using System;
using System.Collections.Generic;
using System.Linq;
using OddsScraper.WebApi.Models;

namespace OddsScraper.WebApi.Services
{
    public class GamesService : IGamesService
    {
        public IEnumerable<GameDto> GetDaysGamesInfo()
            => FSharp.Scraping.FutureGamesDownload.downloadAllDayGameInfos(DateTime.Now).Select(GameDto.Create);

        public IEnumerable<GameDto> GetGameInfos(double timeSpan)
            => FSharp.Scraping.FutureGamesDownload.downloadGameInfos(DateTime.Now, timeSpan).Select(GameDto.Create);

        public IEnumerable<GameDto> GetGames(double timeSpan) 
            => FSharp.Scraping.FutureGamesDownload.downloadGames(DateTime.Now, timeSpan).Select(GameDto.Create);

        public GameDto GetGame(string gameLink) => GameDto.Create(FSharp.Scraping.FutureGamesDownload.readGameFromLink(gameLink));
    }
}
