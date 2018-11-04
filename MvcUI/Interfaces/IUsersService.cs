using System.Threading.Tasks;
using System.Collections.Generic;
using MvcUI.Models;

namespace MvcUI.Interfaces
{
    public interface IUsersService
    {
        Task<User> GetByUserNameAsync(string userName);
        Task<List<User>> GetAllAsync();
        User Add(User user);
        User Update(string userName, User user);
        bool Delete(string userName);
    }
}    