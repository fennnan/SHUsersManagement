using System.ComponentModel.DataAnnotations;

namespace RestServer.Model
{
    public class User
    {
        public long Id {get;set;}
        [Required]
        public string UserName {get;set;}
        public string Password {get;set;}
        public string[] Roles {get;set;}
    }
}