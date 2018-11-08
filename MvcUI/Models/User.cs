using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MvcUI.Models
{
    [DataContract]
    public class UserDto
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public long Id {get;set;}
        [DataMember(Name = "userName", EmitDefaultValue = false)]
        public string UserName {get;set;}
        [DataMember(Name = "roles", EmitDefaultValue = false)]
        public string[] Roles {get;set;}
        public string RolesList
        {
            get
            {
                if (Roles==null || Roles.Length==0)
                    return String.Empty;
                return string.Join(",", Roles);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    Roles = new string[0];
                Roles = value.Split(",");
            }
        }
    }

    public class User : UserDto
    {
        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password {get;set;}
    }

    public class ChangePassword
    {
        public string UserName {get; set;}
        [DataType(DataType.Password)]
        public string Password {get;set;}
        [DataType(DataType.Password)]
        public string VerifyPassword {get;set;}
        [DataType(DataType.Password)]
        public string OldPassword {get;set;}
    }

}