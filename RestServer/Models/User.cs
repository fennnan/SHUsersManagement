using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace RestServer.Model
{
    public class UserDto
    {
        public long Id {get;set;}
        [Required]
        public string UserName {get;set;}
        public string[] Roles {get;set;}
    }

    public class User : UserDto
    {
        public string Password {get;set;}
    }

    public class VerifyUser
    {
        [Required]
        public string User {get;set;}
        public string Password {get;set;}
    }


    public class ChangePassword
    {
        public string Password {get;set;}
        public string VerifyPassword {get;set;}
        [Required]
        public string OldPassword {get;set;}
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}