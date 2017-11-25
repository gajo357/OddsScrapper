using Microsoft.AspNetCore.Mvc;
using OddsScrapper.Mvc.ViewModels;
using OddsWebsite.Models;

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
            return View(new IndexViewModel());
        }

        [HttpGet]
        public IActionResult DownloadResults()
        {
            return View(ResultsViewModel.Instance);
        }
        [HttpPost]
        public IActionResult DownloadResults(ResultsViewModel viewModel)
        {
            viewModel.DownloadResultsAsync();

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