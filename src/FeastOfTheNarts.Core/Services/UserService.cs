using FeastOfTheNarts.Core.Domain.Models;
using FeastOfTheNarts.Core.Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FeastOfTheNarts.Core.Services
{
    public class UserService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<User> GetAllUsers() => _userRepository.GetAll();

        public bool RegisterUser(string username, string password, string email, string phone)
        {
            var users = _userRepository.GetAll();

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

            _userRepository.AddUser(newUser);

            return true;
        }

        //проверка
        public User? VerifyUser(string email, string password)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null) return null;
            
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}