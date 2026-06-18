using FeastOfTheNarts.Core.Domain.Models;
using FeastOfTheNarts.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FeastOfTheNarts.Infrastructure.Repositories
{
    public class UserRepositoryJSON : IUserRepository
    {
        private readonly List<User> _users;
        private const string _filepath = "Data/users.json";

        public UserRepositoryJSON()
        {
            _users = ReadJSON();
        }

        public void AddUser(User user)
        {
            _users.Add(user);
            WriteJSON(_users);
        }

        public void UpdateUser(User user)
        {
            var existingUser = this.GetById(user.Id);
            if (existingUser != null)
            {
                existingUser.Username = user.Username;
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.Email = user.Email;
                existingUser.Wins = user.Wins;
                existingUser.Losses = user.Losses;
            }

            WriteJSON(_users);
        }

        public void DeleteUserById(Guid id)
        {
            _users.RemoveAll(u => u.Id == id);
            WriteJSON(_users);
        }
        public void DeleteUserByEmail(string email)
        {
            _users.RemoveAll(u => u.Email == email);
            WriteJSON(_users);
        }

        public List<User> GetAll() => [.. _users];
        public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);
        public User? GetByEmail(string email) => _users.FirstOrDefault(u => u.Email == email);

        static private List<User> ReadJSON()
        {
            List<User> users = [];

            if (!File.Exists(_filepath))
                File.Create(_filepath).Close();

            string json = File.ReadAllText(_filepath);
            if (!string.IsNullOrEmpty(json))
                users = JsonSerializer.Deserialize<List<User>>(json) ?? [];

            return users;
        }
        static private void WriteJSON(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filepath, json);
        }
    }
}
