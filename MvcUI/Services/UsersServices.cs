using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

using MvcUI.Models;
using MvcUI.Interfaces;
using MvcUI.Helpers;
using Microsoft.Extensions.Logging;

namespace MvcUI.Services
{
    public class UsersService  : IUsersService, IAuthenticateService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private static readonly HttpClient client = new HttpClient();
        private static int _lastID=0;
        private List<User> UserList {get;} = new List<User>();
        public UsersService(IOptions<AppSettings> appSettings, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<UsersService> logger)
        {
            _appSettings=appSettings.Value;
            _httpContextAccessor=httpContextAccessor;
            _logger=logger;

            Uri baseUri = new Uri(_appSettings.UsersApiURL);
            client.BaseAddress=baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.ConnectionClose = false;
        }

        #region IUsersService
        public async Task<User> GetByUserNameAsync(string userName)
        {
            var serializer = new DataContractJsonSerializer(typeof(User));

            // var streamTask = client.GetStreamAsync($"api/Users/{userName}");
            // var retUser = serializer.ReadObject(await streamTask) as User;
            User retUser = null;
            HttpResponseMessage response = await client.GetAsync($"api/Users/{userName}");
            if (!response.IsSuccessStatusCode)
            {
                retUser = await response.Content.ReadAsAsync<User>();
            }

            return retUser;
        }
        public async Task<List<User>> GetAllAsync()
        {
            var serializer = new DataContractJsonSerializer(typeof(List<User>));
            client.DefaultRequestHeaders.Accept.Clear();
            // client.DefaultRequestHeaders.Accept.Add(
            //     new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            // client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync($"{_appSettings.UsersApiURL}/Users");
            var streamTask = client.GetStreamAsync($"{_appSettings.UsersApiURL}/Users");
            var userList = serializer.ReadObject(await streamTask) as List<User>;

            return userList;
        }
        public User Add(User user)
        {
            var item = UserList.Find(x=>x.UserName.ToLower()==user.UserName.ToLower());
            if (item != null)
                throw new Exception("The user name already exists");
            user.Id = ++_lastID;
            UserList.Add(user);
            return user;
        }
        public User Update(string userName, User user)
        {
            var item = UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
            if (item == null)
                throw new Exception("The user is not found");
            var checkItem = UserList.Find(x=>x.UserName.ToLower()==user.UserName.ToLower());
            if (checkItem != null)
                throw new Exception("The user name already exists");
            item.UserName= user.UserName;
            item.Roles=user.Roles;
            item.Password=user.Password;
            return item;
        }

        public bool Delete(string userName)
        {
            var item = UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
            if (item == null)
                throw new Exception("The user is not found");
            return UserList.Remove(item);
        }
        #endregion IUsersService
        #region IAuthenticateService
        public async Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
        {
            _logger.LogDebug($"Signin for {user.UserName}");
            var dbUserData = await GetByUserNameAsync(user.UserName);
            // var dbUserData = new User {UserName=user.UserName, Password=user.Password};
            _logger.LogDebug("After Getbuusername");
            if (dbUserData == null)
                throw new Exception("User or password incorrect");

            _logger.LogDebug($"User Found {dbUserData?.UserName}!");
            if (dbUserData?.Password != user.Password)
            {
                SignOut(httpContext);
                throw new Exception("User or password not valid");
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaims(this.GetUserClaims(dbUserData));
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            // var iaut = httpContext.RequestServices.GetService(typeof(IAuthenticationService));
            _logger.LogDebug("Before httpcontext.signin");
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = isPersistent });
            _logger.LogDebug("After httpcontext.signin");
            //return true;
        }
        public async void SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private IEnumerable<Claim> GetUserClaims(User user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserName));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }

        private IEnumerable<Claim> GetUserRoleClaims(User user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Role, "test"));
            if (user.Roles!=null)
                foreach (var item in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, item));
                }
            return claims;
        }
        #endregion IAuthenticateService
    }
}    