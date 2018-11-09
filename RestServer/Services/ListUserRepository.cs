using System;
using System.Collections.Generic;
using RestServer.Model;
using RestServer.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;

namespace RestServer.Services
{
    public class ListUserRepository  : IUserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static int _lastID=0;
        private List<User> UserList {get;} = new List<User>();
        public ListUserRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor=httpContextAccessor;
        }
        public User GetByUserName(string userName)
        {
            return UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
        }
        public List<User> GetAll()
        {
            return UserList;
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
            var item = GetByUserName(userName);// UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
            if (item == null)
                throw new Exception("The user is not found");
            if (userName.ToLower()!=user.UserName.ToLower())
            {
                var checkItem = UserList.Find(x=>x.UserName.ToLower()==user.UserName.ToLower());
                if (checkItem != null)
                    throw new Exception("The user name already exists");
            }
            item.UserName= user.UserName;
            item.Roles=user.Roles;
            return item;
        }

        public bool Delete(string userName)
        {
            var item = UserList.Find(x=>x.UserName.ToLower()==userName.ToLower());
            if (item == null)
                throw new Exception("The user is not found");
            return UserList.Remove(item);
        }

        #region security
        public User Verify(VerifyUser user)
        {
            return Verify(user.User, user.Password);
        }
        private User Verify(string userName, string password)
        {
            var item = GetByUserName(userName);
            if (item?.Password != password)
                item = null;
            return item;
        }
        public bool UpdatePassword(string userName, ChangePassword pwdDto)
        {
            if (pwdDto.Password != pwdDto.VerifyPassword)
                return false;
            User item = null;
            if (!IsAdmin())
            {
                item = Verify(userName, pwdDto.OldPassword);
                if (item == null)
                    return false;
            }
            else
            {
                item = GetByUserName(userName);
            }
            item.Password=pwdDto.Password;
            return true;
        }

        private bool IsAdmin()
        {
            string authHeader = GetBAHeader();
            if (authHeader != null)
            {
                // Get the encoded username and password
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                // Decode from Base64 to string
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                // Split username and password
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];
                var user = Verify(username, password);
                foreach (var item in user?.Roles)
                {
                    if (item.ToUpper() == "ADMIN")
                        return true;
                }
            }
            return false;
        }
        private string GetBAHeader()
        {
            return _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault(header => header.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
        }
        #endregion security
    }
}    