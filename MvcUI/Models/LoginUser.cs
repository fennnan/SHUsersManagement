using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MvcUI.Models
{
    public class LoginUser
    {
        [Required]
        public string UserName {get;set;}
        [Required]
        [DataType(DataType.Password)]
        public string Password {get;set;}
        public bool RememberMe { get; set; }=false;
    }
}