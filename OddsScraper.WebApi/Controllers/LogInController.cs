using Microsoft.AspNetCore.Mvc;
using OddsScraper.WebApi.Models;
using OddsScraper.WebApi.Services;
using System.Threading.Tasks;

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
        public async Task<ActionResult<string>> Post([FromBody] UserDto user) 
            => await LoginService.LogInAsync(user.Username, user.Password);
    }
}