namespace OddsScraper.WebApi.Services
{
    public class UserLoginService : IUserLoginService
    {
        public bool IsUserLoggedIn(string username) => true;

        public void LogIn(string username, string password) 
            => FSharp.Scraping.CanopyExtensions.loginToOddsPortalWithData(username, password);
    }
}
