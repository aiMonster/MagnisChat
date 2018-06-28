﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Account
{
    public class LoginRequest
    {
        [Required]             
        public string Login { get; set; }

        [Required]       
        public string Password { get; set; }       
    }
}
