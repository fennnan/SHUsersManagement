using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MvcUI.Models;
using MvcUI.Interfaces;
using MvcUI.Helpers;

namespace MvcUI.Services
{
    public class UsersService  : IUsersService
    {
        private readonly AppSettings _appSettings;
        private static readonly HttpClient client = new HttpClient();
        private static int _lastID=0;
        private List<User> UserList {get;} = new List<User>();
        public UsersService(IOptions<AppSettings> appSettings)
        {
            _appSettings=appSettings.Value;
        }
        public async Task<User> GetByUserNameAsync(string userName)
        {
            var serializer = new DataContractJsonSerializer(typeof(User));
            client.DefaultRequestHeaders.Accept.Clear();
            // client.DefaultRequestHeaders.Accept.Add(
            //     new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            // client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var streamTask = client.GetStreamAsync($"{_appSettings.UsersApiURL}/Users/{userName}");
            var retUser = serializer.ReadObject(await streamTask) as User;

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
    }
}    