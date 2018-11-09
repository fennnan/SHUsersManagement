using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcUI.Models;
using MvcUI.Interfaces;
using MvcUI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace MvcUI.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private IUsersService _userSvc;
        private IAuthenticateService _authSvc;

        public UsersController(IUsersService userSvc, IAuthenticateService authSvc)
        {
            _userSvc=userSvc;
            _authSvc=authSvc;
        }

        // GET: Users
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _userSvc.GetAllAsync());
            }
            catch(Exception ex)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }
            if (!_authSvc.CanChangePassword(HttpContext, userName))
            {
                return Unauthorized();
            }
            var user = await _userSvc.GetByUserNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([Bind("UserName,Password,Roles,RolesList")] User user)
        {
            if (ModelState.IsValid)
            {
                await _userSvc.Add(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }

            var user = await _userSvc.GetByUserNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(long id, [Bind("Id,UserName,RolesList")] UserDto user)
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(string userName, [Bind("Id,UserName,RolesList")] UserDto user)
        {
//            if (id != user.Id)
            if (string.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _userSvc.Update(userName, new User{ UserName=user.UserName, Roles=user.Roles });
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }

            var user = await _userSvc.GetByUserNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(string userName)
        {
            // var user = await _context.Users.FindAsync(id);
            // _context.Users.Remove(user);
            // await _context.SaveChangesAsync();

            if (String.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }

            var ret = await _userSvc.Delete(userName);
            if (!ret)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangePassword(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }
            if (!_authSvc.CanChangePassword(HttpContext, userName))
            {
                return Unauthorized();
            }
            var user = await _userSvc.GetByUserNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            ChangePassword cp = new ChangePassword { UserName=user.UserName };

            return View(cp);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string userName, ChangePassword changePassword)
        {
            if (String.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }
            if (!_authSvc.CanChangePassword(HttpContext, userName))
            {
                return Unauthorized();
            }
            try
            {
                var ret = await _userSvc.UpdatePassword(changePassword);
                if (!ret)
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                // ModelState.AddModelError("summary", ex.Message);
                // return View(changePassword);
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ex.Message);
            }

            return RedirectToAction(nameof(Details), new {UserName = userName} );
        }
    }
}
