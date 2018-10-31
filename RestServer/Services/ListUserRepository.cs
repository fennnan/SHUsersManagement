using System;
using System.Collections.Generic;
using RestServer.Model;
using RestServer.Interfaces;

namespace RestServer.Services
{
    public class ListUserRepository  : IUserRepository
    {
        private static int _lastID=0;
        private List<User> UserList {get;} = new List<User>();
        public ListUserRepository()
        {

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
    }
}    