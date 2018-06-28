using Common.DTO.Account;
using Common.DTO.Communication;
using Managers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagnisChatAPI.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountManager _accountManger;

        public AccountController(IAccountManager accountManager)
        {
            _accountManger = accountManager;
        }

        [HttpPost("Token")]
        public IActionResult Token([FromBody]LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity =  _accountManger.GetIdentity(request.Login, request.Password);
            if (identity.Error != null)
            {
                return StatusCode(identity.Error.ErrorCode, identity);
            }

            var response = _accountManger.GetToken(identity.Data);
            return Ok(new ResponseDTO<TokenResponse>() { Data = response });
        }

        [Authorize]
        [HttpPost("SocketToken")]        
        public IActionResult SocketToken()
        {
            var userId = new Guid(User.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value);
            var token = _accountManger.GetSocketToken(userId);
            return Ok(token);
        }

        [Authorize]
        [HttpGet("Users/{id}")]
        public IActionResult GetUser([FromRoute] Guid id)
        {
            var response = _accountManger.GetUser(id);
            if(response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }
            
            return Ok(response);
        }
    }
}
