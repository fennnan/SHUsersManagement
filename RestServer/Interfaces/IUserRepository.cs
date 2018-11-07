using System.Collections.Generic;
using RestServer.Model;

namespace RestServer.Interfaces
{
    public interface IUserRepository
    {
        User GetByUserName(string userName);
        List<User> GetAll();
        User Add(User user);
        User Update(string userName, User user);
        bool Delete(string userName);
        User Verify(VerifyUser user);
        bool UpdatePassword(string userName, ChangePassword pwdDto);
    }
}    