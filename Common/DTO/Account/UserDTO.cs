using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Account
{
    public class UserDTO
    {     
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public UserDTO()
        {

        }

        public UserDTO(string login, string password, Guid guid)
        {
            Login = login;
            Password = password;
            Id = guid;
        }


    }
}
