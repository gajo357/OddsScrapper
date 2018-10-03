using OddsScraper.WebApi.Models;
using System.Collections.Generic;

namespace OddsScraper.WebApi.Services
{
    public interface IGamesService
    {
        GameDto GetGame(string gameLink, string user);
        IEnumerable<GameDto> GetGames(double timeSpan, string user);
        IEnumerable<GameDto> GetDaysGamesInfo(string user);
        IEnumerable<GameDto> GetGameInfos(double timeSpan, string user);
    }
}
