using OddsScraper.WebApi.Models;
using System.Collections.Generic;

namespace OddsScraper.WebApi.Services
{
    public interface IGamesService
    {
        IEnumerable<GameDto> GetGames(double timeSpan);
    }
}
