using System.Diagnostics;
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

        [Authorize]
        public IActionResult Index()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpPost]
        public IActionResult Index(IndexViewModel viewModel)
        {
            EmailService.SendEmail(contactInfo);

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
