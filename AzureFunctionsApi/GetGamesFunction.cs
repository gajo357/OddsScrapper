using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctionsApi
{
    public static class GetGamesFunction
    {
        [FunctionName("GetGames")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getGames/{count}")]HttpRequest req,
            ExecutionContext context,
            ILogger log, int count)
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
    }
}
