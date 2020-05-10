using ParkyAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUserUnique(string Username);
        User Authenticate(string username, string password);
        User Register(string userName, string password);
    }
}
