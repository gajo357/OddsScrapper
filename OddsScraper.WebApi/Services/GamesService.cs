using System;
using System.Collections.Generic;
using System.Linq;
using OddsScraper.WebApi.Models;

namespace OddsScraper.WebApi.Services
{
    public class GamesService : IGamesService
    {
        public IEnumerable<GameDto> GetGames(double timeSpan) 
            => FSharp.Scraping.FutureGamesDownload.downloadGames(DateTime.Now, timeSpan).Select(GameDto.Create);
    }
}
