using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using RestServer.Model;

namespace RestServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        internal List<Rol> RolList = new List<Rol>() {
            new Rol {RolName = "Administrator"},
            new Rol {RolName = "Rol1"},
            new Rol {RolName = "Rol2"},
            new Rol {RolName = "Rol3"},
        };
        // GET api/values
        [HttpGet]
        public ActionResult<List<Rol>> Get()
        {
            return RolList;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
