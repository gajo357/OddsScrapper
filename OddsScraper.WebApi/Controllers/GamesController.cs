using Microsoft.AspNetCore.Mvc;
using OddsScraper.WebApi.Models;
using OddsScraper.WebApi.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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

        // GET api/games/games?timeSpan=30
        [HttpGet]
        public async Task<ActionResult<List<GameDto>>> Games([FromQuery]double? timeSpan, [FromHeader]string authorization)
        {
            var resultTask = timeSpan.HasValue && timeSpan > 0 ?
                GamesService.GetGameInfosAsync(timeSpan.Value, authorization) :
                GamesService.GetDaysGamesInfoAsync(authorization);

            var result = await resultTask;
            
            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        // POST api/games/singleGame
        [HttpPost(Name = "singleGame")]
        public async Task<ActionResult<GameDto>> SingleGame([FromBody]GameLink gameLink, [FromHeader]string authorization)
        {
            var result = await GamesService.GetGameAsync(gameLink.Link, authorization);
            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        public class GameLink
        {
            [Required]
            public string Link { get; set; }
        }
    }
}
