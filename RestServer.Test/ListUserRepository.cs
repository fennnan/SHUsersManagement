using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft;
using Microsoft.AspNetCore.Mvc;
using RestServer.Controllers;
using RestServer.Model;
using RestServer.Interfaces;

namespace RestServer.Test
{
    public class ListUserRepositoryTest
    {
        // TODO: test functions
        #region Fill mockups
        private List<User> GetListOfUsers()
        {
            return new List<User>() {
                new User {Id=1, UserName = "Admin", Password="admin1", Roles=new string[] {"Administrator"}},
                new User {Id=2, UserName = "User1", Password="pass1", Roles=new string[] {"Rol1"}},
                new User {Id=3, UserName = "User2", Password="pass2", Roles=new string[] {"Rol2"}},
                new User {Id=4, UserName = "User3", Password="pass3", Roles=new string[] {"Rol3"}},
            };            
        }
        #endregion Fill mockups
    }
}
