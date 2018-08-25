using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OddsScraper.WebApi.Models;
using OddsScraper.WebApi.Services;

namespace OddsScraper.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private IGamesService GamesService { get; }

        public GamesController(IGamesService gamesService)
        {
            GamesService = gamesService;
        }

        // GET api/games/5.0
        [HttpGet("{timeSpan}")]
        public ActionResult<List<GameDto>> Get(double timeSpan) => GamesService.GetGames(timeSpan).ToList();
    }
}
