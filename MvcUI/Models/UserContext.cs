// UserContext.cs
using MvcUI.Models;
using Microsoft.EntityFrameworkCore;

namespace MvcUI.Models
{
    public class UserContext :DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) 
        {

        }

        public DbSet<User> Users { get; set; }
    }
}