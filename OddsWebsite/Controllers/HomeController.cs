using Microsoft.AspNetCore.Mvc;
using OddsWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using OddsWebsite.ViewModels;

namespace OddsWebsite.Controllers
{
    public class HomeController : Controller
    {
        private IEmailService EmailService { get; }
        private IArchiveDataRepository Repository { get; }

        public HomeController(IEmailService emailService, IArchiveDataRepository repository)
        {
            EmailService = emailService;
            Repository = repository;
        }

        public IActionResult Index()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [Authorize]
        public IActionResult Results()
        {
            ViewData["Message"] = "Your results page.";

            return View();
        }

        [HttpPost]
        public IActionResult Index(IndexViewModel viewModel)
        {
            //EmailService.SendEmail();

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var results = Repository.GetResultsForUser(User.Identity.Name);

                return Ok(results);
            }
            catch
            {

            }

            ViewData["Message"] = "Your contact page.";

            return View();
        }
    }
}
