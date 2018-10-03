namespace OddsScraper.WebApi.Services
{
    public interface IUserLoginService : IHashService
    {
        bool IsUserLoggedIn(string username);
        string LogIn(string username, string password);
    }
}
