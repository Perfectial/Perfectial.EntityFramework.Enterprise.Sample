using Perfectial.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.Common
{
    public class UserDto
    {
        public int Id { get; set; }

        public UserType UserType { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int CreditScore { get; set; }

        public bool WelcomeEmailSent { get; set; }
    }
}
