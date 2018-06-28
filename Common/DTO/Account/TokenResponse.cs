using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Account
{
    public class TokenResponse
    {
        public string Token { get; set; }  
        public string UserLogin { get; set; }
        public string UserId { get; set; }
    }
}
