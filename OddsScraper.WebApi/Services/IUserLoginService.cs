using System.Threading.Tasks;

namespace OddsScraper.WebApi.Services
{
    public interface IUserLoginService : IHashService
    {
        bool IsUserLoggedIn(string username);
        Task<string> LogInAsync(string username, string password);
    }
}
