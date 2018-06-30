using Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Account
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Login { get; set; }

        public UserProfile()
        {

        }

        public UserProfile(UserEntity entity)
        {
            Id = entity.Id;
            Login = entity.Login;
        }
    }
}
