using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OddsWebsite.Models;
using OddsWebsite.ViewModels;
using System.Threading.Tasks;

namespace OddsWebsite.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<OddsAppUser> _signInManager;

        public LoginController(SignInManager<OddsAppUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Home", "Index");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password, true, false);

                if(signInResult.Succeeded)
                {
                    if(string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("Home", "Index");
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Wrong Username or Password");
                }
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            if(User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction("Home", "Index");
        }
    }
}
