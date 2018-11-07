using System.Threading.Tasks;
using System.Collections.Generic;
using MvcUI.Models;

namespace MvcUI.Interfaces
{
    public interface IUsersService
    {
        Task<UserDto> GetByUserNameAsync(string userName);
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto> Add(User user);
        Task<User> Update(string userName, User user);
        Task<bool> Delete(string userName);
    }
}    