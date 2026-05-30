using FeastOfTheNarts.Core.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FeastOfTheNarts.Core.Services
{
    public class JsonUserService
    {
        private readonly string _filePath = "users.json";
        private readonly PasswordHasher<User> _passwordHasher = new();


        public List<User> GetAllUsers()
        {
            if (!File.Exists(_filePath)) return new List<User>();

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                // На случай, если файл поврежден или пуст
                return new List<User>();
            }
        }


        public bool RegisterUser(string username, string password, string email, string phone)
        {
            var users = GetAllUsers();

            //не занят ли логин или почта
            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return false;

            if (users.Any(u => !string.IsNullOrEmpty(u.Email) && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                return false;

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                Wins = 0,
                Losses = 0
            };

            
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

            users.Add(newUser);


            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
            return true;
        }

        //проверка
        public User? VerifyUser(string username, string password)
        {
            var users = GetAllUsers();
            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null) return null;

            
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}