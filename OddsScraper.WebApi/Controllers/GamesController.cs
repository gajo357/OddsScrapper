using Microsoft.AspNetCore.Mvc;
using OddsScraper.WebApi.Models;
using OddsScraper.WebApi.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        // GET api/games/games
        [HttpGet]
        public ActionResult<List<GameDto>> Games([FromQuery]double? timeSpan, [FromQuery]string user)
        {
            var result = (timeSpan.HasValue ?
                GamesService.GetGameInfos(timeSpan.Value, user) :
                GamesService.GetDaysGamesInfo(user)).ToArray();
            
            if (result.Any())
                return Ok(result);

            return BadRequest();
        }

        // POST api/games/singleGame
        [HttpPost(Name = "singleGame")]
        public ActionResult<GameDto> SingleGame([FromBody]GameLink gameLink)
        {
            var result = GamesService.GetGame(gameLink.Link, gameLink.User);
            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        public class GameLink
        {
            [Required]
            public string Link { get; set; }
            [Required]
            public string User { get; set; }
        }
    }
}
