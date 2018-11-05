using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MvcUI.Models;
using MvcUI.Interfaces;
using MvcUI.Helpers;
using Microsoft.Extensions.Logging;

namespace MvcUI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAuthenticateService _authSvc;
        private readonly ILogger _logger;

        public AccountController(IAuthenticateService authSvc,
            ILogger<AccountController> logger)
        {
            _authSvc = authSvc;
            _logger=logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await Logout();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LoginUser form, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(form);
            try
            {
                //authenticate
                var user = new User()
                {
                    UserName = form.UserName,
                    Password = form.Password
                };
                _logger.LogDebug($"LogIn for {form.UserName}");
                await Task.Run(()=> _authSvc.SignIn(this.HttpContext, user, form.RememberMe));
                _logger.LogDebug("After signin");
                return Redirect(returnUrl);// ToAction(nameof(HomeController.Index), "Home", null);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("summary", ex.Message);
                return View(form);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await Task.Run(()=> _authSvc.SignOut(HttpContext));
            //_logger.LogDebug("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
