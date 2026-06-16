using FeastOfTheNarts.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeastOfTheNarts.Core.Domain.RepositoryInterfaces
{
    public interface IUserRepository
    {
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUserById(Guid id);
        void DeleteUserByEmail(string email);
        List<User> GetAll();
        User? GetById(Guid id);
        User? GetByEmail(string email);
    }
}
