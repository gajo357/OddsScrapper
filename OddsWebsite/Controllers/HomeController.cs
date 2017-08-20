using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OddsWebsite.Models;

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

        public IActionResult Results()
        {

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactInfoModel contactInfo)
        {
            EmailService.SendEmail(contactInfo);

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
