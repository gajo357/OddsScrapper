using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureFunctionsApi
{
    public static class Functions
    {
        [FunctionName("GetGames")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getGames/{count}")]HttpRequest req,
            ExecutionContext context, ILogger log, int count)
        {
            try
            {
                var path = Path.GetFullPath(Path.Combine(context.FunctionDirectory, "..\\"));
                var games = await OddsScraper.FSharp.CommonScraping.Downloader.DownloadFromWidget(path);
                return new OkObjectResult(games.Take(count > 0 ? count : int.MaxValue));
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        [FunctionName("CalculateAmounts")]
        public static async Task<IActionResult> CalculateAmounts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CalculateAmounts")]HttpRequestMessage req,
            ExecutionContext context, ILogger log)
        {
            try
            {
                var model = await req.Content.ReadAsAsync<OddsModel>();

                var result = 
                    OddsScraper.FSharp.CommonScraping.OddsManipulation.amountsToBet(
                        OddsToOdds(model.MyOdds), OddsToOdds(model.BookerOdds), model.Amount);

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        public class OddsModel
        {
            public double Amount { get; set; }
            public Odds MyOdds { get; set; }
            public Odds BookerOdds { get; set; }
        }
        public class Odds
        {
            public double Home { get; set; }
            public double Draw { get; set; }
            public double Away { get; set; }
        }

        private static OddsScraper.FSharp.CommonScraping.Models.Odds OddsToOdds(Odds odds) 
            => new OddsScraper.FSharp.CommonScraping.Models.Odds(odds.Home, odds.Draw, odds.Away);
    }
}
