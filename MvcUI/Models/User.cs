using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MvcUI.Models
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public long Id {get;set;}
        [DataMember(Name = "userName", EmitDefaultValue = false)]
        public string UserName {get;set;}
        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password {get;set;}
        [DataMember(Name = "roles", EmitDefaultValue = false)]
        public string[] Roles {get;set;}
    }
}