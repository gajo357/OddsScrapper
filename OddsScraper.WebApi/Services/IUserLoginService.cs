namespace OddsScraper.WebApi.Services
{
    public interface IUserLoginService
    {
        bool IsUserLoggedIn(string username);
        void LogIn(string username, string password);
    }
}
