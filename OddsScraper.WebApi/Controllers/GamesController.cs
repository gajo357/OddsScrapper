using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OddsScraper.WebApi.Models;
using OddsScraper.WebApi.Services;

namespace OddsScraper.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private IGamesService GamesService { get; }

        public GamesController(IGamesService gamesService)
        {
            GamesService = gamesService;
        }

        // GET api/games/5.0
        [HttpGet("{timeSpan}", Name = "games")]
        [ActionName("games")]
        public ActionResult<List<GameDto>> GamesInSpan(double timeSpan) => GamesService.GetGames(timeSpan).ToList();

        [HttpGet("", Name = "dayGames")]
        [ActionName("dayGames")]
        public ActionResult<List<GameDto>> DayGames() => GamesService.GetDaysGamesInfo().ToList();

        [HttpPost("", Name = "singleGame")]
        [ActionName("singleGame")]
        public ActionResult<GameDto> SingleGame(GameLink gameLink) => GamesService.GetGame(gameLink.Link);

        public class GameLink
        {
            public string Link { get; set; }
        }
    }
}
