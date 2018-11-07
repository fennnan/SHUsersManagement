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
        private static HttpClient client = null;

        public UsersService(IOptions<AppSettings> appSettings, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<UsersService> logger)
        {
            _appSettings=appSettings.Value;
            _httpContextAccessor=httpContextAccessor;
            _logger=logger;

            Uri baseUri = new Uri(_appSettings.UsersApiURL);
            if (client==null)
            {
                client=new HttpClient();
                client.BaseAddress=baseUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.ConnectionClose = false;
            }
        }

        #region IUsersService
        public async Task<UserDto> GetByUserNameAsync(string userName)
        {
            // var serializer = new DataContractJsonSerializer(typeof(User));
            // var streamTask = client.GetStreamAsync($"api/Users/{userName}");
            // var retUser = serializer.ReadObject(await streamTask) as User;
            UserDto retUser = null;
            HttpResponseMessage response = await client.GetAsync($"api/Users/{userName}");
            if (response.IsSuccessStatusCode)
            {
                retUser = await response.Content.ReadAsAsync<UserDto>();
            }

            return retUser;
        }
        public async Task<List<UserDto>> GetAllAsync()
        {
            // var serializer = new DataContractJsonSerializer(typeof(List<User>));

            // var streamTask = client.GetStreamAsync($"{_appSettings.UsersApiURL}/Users");
            // var userList = serializer.ReadObject(await streamTask) as List<UserDto>;
            HttpResponseMessage response = await client.GetAsync($"api/Users");
            List<UserDto> userList;
            if (response.IsSuccessStatusCode)
            {
                userList = await response.Content.ReadAsAsync<List<UserDto>>();
            }
            else
                userList=new List<UserDto>();

            return userList;
        }
        public async Task<UserDto> Add(User user)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"api/Users", user );
            UserDto retUser = null;
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsAsync<string>();
                if (msg != null)
                    throw new Exception(msg);
                response.EnsureSuccessStatusCode();
            }
            retUser = await response.Content.ReadAsAsync<UserDto>();
            return retUser;
        }
        public async Task<User> Update(string userName, User user)
        {
            _logger.LogDebug($"Update {userName}");
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"api/Users/{userName}", new { UserName = user.UserName, Roles = user.Roles} );
            _logger.LogDebug("After update");            

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsAsync<string>();
                if (msg != null)
                    throw new Exception(msg);
                response.EnsureSuccessStatusCode();
            }
            // UserDto dbUserData = await response.Content.ReadAsAsync<UserDto>();;
            return user;
            // var item = UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
            // if (item == null)
            //     throw new Exception("The user is not found");
            // var checkItem = UserList.Find(x=>x.UserName.ToLower()==user.UserName.ToLower());
            // if (checkItem != null)
            //     throw new Exception("The user name already exists");
            // item.UserName= user.UserName;
            // item.Roles=user.Roles;
            // item.Password=user.Password;
            // return item;
        }

        public async Task<bool> Delete(string userName)
        {
            // var item = UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
            // if (item == null)
            //     throw new Exception("The user is not found");
            // return UserList.Remove(item);
            HttpResponseMessage response = await client.DeleteAsync($"api/Users/{userName}");
            // UserDto retUser = null;
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsAsync<string>();
                if (msg != null)
                    throw new Exception(msg);
                response.EnsureSuccessStatusCode();
            }
            // retUser = await response.Content.ReadAsAsync<UserDto>();
            return true;

        }
        #endregion IUsersService
        #region IAuthenticateService
        public async Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
        {
            _logger.LogDebug($"Signin for {user.UserName}");
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "api/Users/Verify", new { User = user.UserName, Password = user.Password} );
            _logger.LogDebug("After verify");            

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsAsync<string>();
                if (msg != null)
                    throw new Exception(msg);
                response.EnsureSuccessStatusCode();
            }
            UserDto dbUserData = await response.Content.ReadAsAsync<UserDto>();;

            // var dbUserData = await GetByUserNameAsync(user.UserName);
            // var dbUserData = new User {UserName=user.UserName, Password=user.Password};
            // _logger.LogDebug("After Getbyusername");
            if (dbUserData == null)
                throw new Exception("User or password incorrect");

            _logger.LogDebug($"User Found {dbUserData?.UserName}!");
            // if (dbUserData?.Password != user.Password)
            // {
            //     SignOut(httpContext);
            //     throw new Exception("User or password not valid");
            // }

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

        private IEnumerable<Claim> GetUserClaims(UserDto user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserName));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }

        private IEnumerable<Claim> GetUserRoleClaims(UserDto user)
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