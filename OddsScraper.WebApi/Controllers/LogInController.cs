using Microsoft.AspNetCore.Mvc;
using OddsScraper.WebApi.Models;
using OddsScraper.WebApi.Services;

namespace OddsScraper.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private IUserLoginService LoginService { get; }

        public LogInController(IUserLoginService loginService)
        {
            LoginService = loginService;
        }

        [HttpGet("{user}")]
        public ActionResult<bool> Get(string user) => LoginService.IsUserLoggedIn(user);

        [HttpPost]
        public ActionResult<string> Post([FromBody] UserDto user) => LoginService.LogIn(user.Username, user.Password);
    }
}