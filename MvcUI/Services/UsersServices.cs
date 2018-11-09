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
using System.Threading;
using System.Net;
using System.Linq;
using System.Text;

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
            response.EnsureSuccessStatusCode();
            List<UserDto> userList = await response.Content.ReadAsAsync<List<UserDto>>();

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
            return user;
        }

        public async Task<bool> Delete(string userName)
        {
            HttpResponseMessage response = await client.DeleteAsync($"api/Users/{userName}");
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsAsync<string>();
                if (msg != null)
                    throw new Exception(msg);
                response.EnsureSuccessStatusCode();
            }
            return true;

        }
        
        public async Task<bool> UpdatePassword(ChangePassword changePassword)
        {
            _logger.LogDebug($"ChangePassword {changePassword.UserName}");
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"api/Users/{changePassword.UserName}/Password", 
                new { 
                    OldPassword = changePassword.OldPassword??"*",
                    Password = changePassword.Password,
                    VerifyPassword = changePassword.VerifyPassword,
                } );
            _logger.LogDebug("After ChangePassword");            

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsAsync<string>();
                if (msg != null)
                    throw new Exception(msg);
                response.EnsureSuccessStatusCode();
            }
            SetBasicAuthentication(changePassword.UserName, changePassword.Password);
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

            if (dbUserData == null)
                throw new Exception("User or password incorrect");

            _logger.LogDebug($"User Found {dbUserData?.UserName}!");

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaims(this.GetUserClaims(dbUserData));
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            // var iaut = httpContext.RequestServices.GetService(typeof(IAuthenticationService));
            _logger.LogDebug("Before httpcontext.signin");
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = isPersistent });
            SetBasicAuthentication(user.UserName, user.Password);
            _logger.LogDebug("After httpcontext.signin");
            //return true;
        }
        public async void SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            RemoveBasicAuthentication();
        }

        public bool CanChangePassword(HttpContext context, string userName)
        {
            if (context.User.IsInRole("ADMIN") || 
                context.User.Identity.Name.ToLower() == userName.ToLower())
                    return true;
            return false;
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

        #region API Basic Authentication
        private void SetBasicAuthentication(string userName, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{userName}:{password}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",Convert.ToBase64String(byteArray));
        }

        private void RemoveBasicAuthentication()
        {
            client.DefaultRequestHeaders.Authorization=null;
        }
        #endregion API Basic Authentication
    }
    public class BasicAuthenticationHandler : DelegatingHandler
    {
        IHttpContextAccessor _httpContextAccessor;
        public BasicAuthenticationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor=httpContextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {

            }
            else
            {

            }
            if (!request.Headers.Contains("X-API-KEY"))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        "You must supply an API key header called X-API-KEY")
                };
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private void RemoveBA(HttpContext context)
        {
            var basicAuthenticationHeader = GetBAHeader(context);
            if (!string.IsNullOrWhiteSpace(basicAuthenticationHeader))
            {
                var b=context.Request.Headers.Remove("Authorization");
            }
        }

        private void SetBA(HttpContext context, string user, string password)
        {
            var basicAuthenticationHeader = GetBAHeader(context);
            var authenticatedUser = BAEncode(user, password);
            if (!string.IsNullOrWhiteSpace(basicAuthenticationHeader))
            {
                var b=context.Request.Headers.Remove("Authorization");
            }
        }

        private string GetBAHeader(HttpContext context)
        {
            return context.Request.Headers["Authorization"]
                .FirstOrDefault(header => header.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
        }

        private string BAEncode(string userName, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{userName}:{password}");
            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
            return Convert.ToBase64String(byteArray);
        }
    }
}    