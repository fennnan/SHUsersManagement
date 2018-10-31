using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using RestServer.Model;
using RestServer.Interfaces;

namespace RestServer.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserRepository UserRepository {get;set;}
        public UsersController(IUserRepository userRepository)
        {
            UserRepository=userRepository;
        }
        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(UserRepository.GetAll());
        }

        // GET api/values/5
        [HttpGet("{userName}", Name = "GetUser")]
        public IActionResult Get(string userName)
        {
            try
            {
                var item = UserRepository.GetByUserName(userName);
                if (item == null)
                    return NotFound();
                return Ok(item);
            }
            catch(Exception ex)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post(User user)
        {
            try
            {
                var dbUuser = UserRepository.GetByUserName(user.UserName);
                if (dbUuser != null)
                    return Conflict($"El nombre de usuario '{user.UserName}' ya existe");
                var item = UserRepository.Add(user);
                if (item == null)
                    throw new Exception("Se ha producido un error interno");
                return CreatedAtRoute("GetUser", new { userName = item.UserName }, item);
            }
            catch(Exception ex)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // PUT api/values/5
        [HttpPut("{userName}")]
        public IActionResult Put(string userName, User user)
        {
            try
            {
                var dbUuser = UserRepository.GetByUserName(user.UserName);
                if (dbUuser != null)
                    return Conflict($"El nombre de usuario '{user.UserName}' ya existe");
                var item = UserRepository.Update(userName, user);
                if (item == null)
                    return NotFound();
                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{userName}")]
        public IActionResult Delete(string userName)
        {
            try
            {
                if (UserRepository.Delete(userName))
                    return NoContent();
                return NotFound();
            }
            catch(Exception ex)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
