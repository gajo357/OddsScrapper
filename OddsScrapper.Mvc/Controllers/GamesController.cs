using Microsoft.AspNetCore.Mvc;
using OddsScrapper.Mvc.ViewModels;
using OddsWebsite.Models;
using System.Threading.Tasks;

namespace OddsScrapper.Mvc.Controllers
{
    public class GamesController : Controller
    {
        public const string ControllerName = "Games";

        private readonly IArchiveDataRepository _repository;

        public GamesController(IArchiveDataRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Title = "Games";

            return View(new IndexViewModel());
        }

        [HttpGet]
        public IActionResult DownloadResults()
        {
            return View(new ResultsViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> DownloadResults(ResultsViewModel viewModel, string submitButton)
        {
            if (submitButton == "cancel")
            {
                viewModel.IsDownloading = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    await viewModel.DownloadResultsAsync();
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult DownloadCommingGames()
        {
            return View(CommingGamesViewModel.Instance);
        }
        [HttpPost]
        public IActionResult DownloadCommingGames(CommingGamesViewModel viewModel)
        {
            viewModel.DownloadResultsAsync();

            return View(viewModel);
        }
    }
}