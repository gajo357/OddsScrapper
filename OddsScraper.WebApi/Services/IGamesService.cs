using OddsScraper.WebApi.Models;
using System.Threading.Tasks;

namespace OddsScraper.WebApi.Services
{
    public interface IGamesService
    {
        Task<GameDto> GetGameAsync(string gameLink, string user);
        GameDto[] GetGames(double timeSpan, string user);
        Task<GameDto[]> GetGameInfosAsync(int? gamesCount, string user);
    }
}
