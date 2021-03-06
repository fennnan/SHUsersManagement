using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestServer.Controllers;
using RestServer.Model;
using RestServer.Interfaces;

namespace RestServer.Test
{
    public class UsersControllerTest
    {
        private static MyProfile _myProfile = new MyProfile();
        private static MapperConfiguration _mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile(_myProfile));
        private static Mapper _mapper = new Mapper(_mapperConfiguration);
        #region Get
        [Fact]
        public void Get_All_ReturnList()
        {

            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetAll()).Returns(GetListOfUsers());
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Get();
            
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<List<UserDto>>(actionResult.Value);
            Assert.Equal(4, model.Count);
        }

        [Fact]
        public void Get_ByUserNameValid_ReturnUser()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetByUserName(It.IsAny<string>())).Returns((string u) => new User {UserName=u});
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Get("User1");
            
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<UserDto>(actionResult.Value);
            Assert.NotNull(user);
            Assert.Equal("User1", user.UserName);
        }

        [Fact]
        public void Get_ByUserNameNotExists_ReturnUser()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetByUserName(It.IsAny<string>())).Returns((string u) => null);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Get("UserN");
            
            var actionResult = Assert.IsAssignableFrom<NotFoundResult>(result);
            Assert.Equal(404, actionResult.StatusCode);
        }
        // TODO: more test functions
        #endregion Get

        #region Verify
        [Fact]
        public void Verify_ValidUserPassword_ReturnUser()
        {
            var mockRepo = new Mock<IUserRepository>();
            // mockRepo.Setup(repo => repo.GetAll()).Returns(GetListOfUsers());
            // mockRepo.Setup(repo => repo.GetByUserName(It.IsAny<string>())).Returns((string u) => new User {UserName=u, Password="pass1"});
            mockRepo.Setup(repo => repo.Verify(It.IsAny<VerifyUser>())).Returns((VerifyUser u)=>
                new User{ UserName=u.User, Password=u.Password}
            );
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Verify(new VerifyUser {User="User1", Password="pass1"});
            
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<UserDto>(actionResult.Value);
            Assert.NotNull(user);
            Assert.Equal("User1", user.UserName);
        }

        [Fact]
        public void Verify_IncorrectPassword_ReturnBadRequest()
        {
            var mockRepo = new Mock<IUserRepository>();
            // mockRepo.Setup(repo => repo.GetAll()).Returns(GetListOfUsers());
            // mockRepo.Setup(repo => repo.GetByUserName(It.IsAny<string>())).Returns((string u) => new User {UserName=u, Password="pass1"});
            mockRepo.Setup(repo => repo.Verify(It.IsAny<VerifyUser>())).Returns((VerifyUser u)=>null);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Verify(new VerifyUser {User="User1", Password="notvalidpassword"});
            
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
            var message = Assert.IsType<string>(actionResult.Value);
            Assert.Equal("Username or password is incorrect", message);
        }
        // TODO: more test functions
        #endregion Verify

        #region UpdatePassword
        [Fact]
        public void UpdatePassword_ValidUserPassword_ReturnNocontent()
        {
            var mockRepo = new Mock<IUserRepository>();
//            mockRepo.Setup(repo => repo.GetAll()).Returns(GetListOfUsers());
            mockRepo.Setup(repo => repo.UpdatePassword(It.IsAny<string>(), It.IsAny<ChangePassword>())).Returns(true);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.UpdatePassword("User1", new ChangePassword {Password="pass2", VerifyPassword="pass2", OldPassword="pass1"});
            
            var actionResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, actionResult.StatusCode);
        }

        [Fact]
        public void UpdatePassword_IncorrectUpdate_ReturnBadRequest()
        {
            var mockRepo = new Mock<IUserRepository>();
//            mockRepo.Setup(repo => repo.GetAll()).Returns(GetListOfUsers());
            mockRepo.Setup(repo => repo.UpdatePassword(It.IsAny<string>(), It.IsAny<ChangePassword>())).Returns(false);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.UpdatePassword("User1", new ChangePassword {Password="pass2", VerifyPassword="pass2", OldPassword="notvalidpassword"});
            
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
            var message = Assert.IsType<string>(actionResult.Value);
            Assert.Equal("Username or password is incorrect", message);
        }
       
        // TODO: more test functions
        #endregion UpdatePassword

        #region Post
        [Fact]
        public void Post_ValidUser_ReturnUser()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Add(It.IsAny<User>())).Returns((User u) => u);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Post(new User {UserName="newUser", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsType<CreatedAtRouteResult>(result);
            var model = Assert.IsAssignableFrom<UserDto>(actionResult.Value);
            Assert.NotNull(model);
            Assert.Equal("newUser", model.UserName);
            Assert.Equal("getuser", actionResult.RouteName.ToLower());
            Assert.NotNull(actionResult.RouteValues["UserName"]);
            Assert.Equal("newUser", actionResult.RouteValues["UserName"]);
        }

        [Fact]
        public void Post_AlreadyExistingUserName_ReturnConflict()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetByUserName(It.IsAny<string>())).Returns((string u) => new User {UserName=u});
            mockRepo.Setup(repo => repo.Add(It.IsAny<User>())).Returns((User)null);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Post(new User {UserName="newUser", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsAssignableFrom<ConflictObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
            Assert.Equal("El nombre de usuario 'newUser' ya existe", actionResult.Value);
        }

        [Fact]
        public void Post_InvalidUser_ReturnException()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Add(It.IsAny<User>())).Returns((User)null);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Post(new User {UserName="newUser", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsType<ObjectResult >(result);
            Assert.Equal(500, actionResult.StatusCode);
            var message = Assert.IsType<string>(actionResult.Value);
            Assert.Equal("Se ha producido un error interno", message);
        }
        // TODO: more test functions
        #endregion Post

        #region Put
        [Fact]
        public void Put_ValidUser_ReturnNocontent()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Update(It.IsAny<string>(), It.IsAny<User>())).Returns((string s, User u) => u);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Put("User1", new User {UserName="UserN", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, actionResult.StatusCode);
        }

        [Fact]
        public void Put_NotExistingUserName_ReturnNotFound()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Update(It.IsAny<string>(), It.IsAny<User>())).Returns((User)null);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Put("User1", new User {UserName="UserN", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsAssignableFrom<NotFoundResult>(result);
            Assert.Equal(404, actionResult.StatusCode);

            // var actionResult = Assert.IsType<ObjectResult>(result);
            // Assert.Equal(500, actionResult.StatusCode);
            // var exc = Assert.IsType<Exception>(actionResult.Value);
            // Assert.Equal("Se ha producido un error interno", exc.Message);
        }

        [Fact]
        public void Put_ChangeToAlreadyExistingUserName_ReturnConflict()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetByUserName(It.IsAny<string>())).Returns((string u) => new User {UserName=u});
            mockRepo.Setup(repo => repo.Update(It.IsAny<string>(), It.IsAny<User>())).Returns((User)null);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Put("User1", new User {UserName="User2", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsAssignableFrom<ConflictObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
            Assert.Equal("El nombre de usuario 'User2' ya existe", actionResult.Value);
        }

        [Fact]
        public void Put_InvalidUser_ReturnException()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Update(It.IsAny<string>(), It.IsAny<User>())).Returns((string s, User u)=>throw new Exception("Test error"));
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Put("User1", new User {UserName="UserN", Roles=new string[] {"Rol1"}});
            
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
            var message = Assert.IsType<string>(actionResult.Value);
            Assert.Equal("Test error", message);
        }
        // TODO: more test functions
        #endregion Put

        #region Delete
        [Fact]
        public void Delete_ValidUserName_ReturnNocontent()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Delete(It.IsAny<string>())).Returns(true);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Delete("User1");
            
            var actionResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, actionResult.StatusCode);
        }

        [Fact]
        public void Delete_InvalidUser_ReturnNotFound()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.Delete(It.IsAny<string>())).Returns(false);
            var controller = new UsersController(mockRepo.Object, _mapper);
            var result = controller.Delete("User1");
            
            var actionResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, actionResult.StatusCode);
        }
        // TODO: more test functions
        #endregion Delete

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

    public class MyProfile :Profile
    {
        public MyProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }

    }
}
