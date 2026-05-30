using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeastOfTheNarts.Core.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
    }
}
