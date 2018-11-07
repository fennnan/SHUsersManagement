using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcUI.Models;

namespace MvcUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "ADMIN,PAGE_1")]
        public IActionResult Page1()
        {
            ViewData["Message"] = "This is page 1.";

            return View();
        }

        [Authorize(Roles = "ADMIN,PAGE_2")]
        public IActionResult Page2()
        {
            ViewData["Message"] = "This is page 2.";

            return View();
        }

        [Authorize(Roles = "ADMIN,PAGE_3")]
        public IActionResult Page3()
        {
            ViewData["Message"] = "This is page 3.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
