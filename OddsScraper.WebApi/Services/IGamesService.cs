using OddsScraper.WebApi.Models;
using System.Collections.Generic;

namespace OddsScraper.WebApi.Services
{
    public interface IGamesService
    {
        GameDto GetGame(string gameLink);
        IEnumerable<GameDto> GetGames(double timeSpan);
        IEnumerable<GameDto> GetDaysGamesInfo();
        IEnumerable<GameDto> GetGameInfos(double timeSpan);
    }
}
