using System.Threading.Tasks;
using System.Collections.Generic;
using MvcUI.Models;
using Microsoft.AspNetCore.Http;

namespace MvcUI.Interfaces
{
    public interface IAuthenticateService
    {
        Task SignIn(HttpContext httpContext, User user, bool isPersistent = false);
        void SignOut(HttpContext httpContext);
    }
}    