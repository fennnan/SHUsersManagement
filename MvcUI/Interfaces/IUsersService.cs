using System.Threading.Tasks;
using System.Collections.Generic;
using MvcUI.Models;
using Microsoft.AspNetCore.Http;

namespace MvcUI.Interfaces
{
    public interface IUsersService
    {
        Task<UserDto> GetByUserNameAsync(string userName);
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto> Add(User user);
        Task<User> Update(string userName, User user);
        Task<bool> Delete(string userName);
        Task<bool> UpdatePassword(ChangePassword changePassword);
    }
}    