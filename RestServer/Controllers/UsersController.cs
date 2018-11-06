using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using RestServer.Model;
using RestServer.Interfaces;

namespace RestServer.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserRepository UserRepository {get;set;}
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            UserRepository=userRepository;
            _mapper=mapper;
        }

        [HttpPost("verify")]
        public IActionResult Verify(VerifyUser user)
        {
            var item = UserRepository.GetByUserName(user.User);
            if (item == null || item.Password != user.Password)
                return BadRequest("Username or password is incorrect");

            return Ok(_mapper.Map<UserDto>(item));
        }

        // GET api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_mapper.Map<List<UserDto>>(UserRepository.GetAll()));
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
                return Ok(_mapper.Map<UserDto>(item));
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
                return CreatedAtRoute("GetUser", new { userName = item.UserName }, _mapper.Map<UserDto>(item));
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
