using Common.DTO.Account;
using Common.DTO.Communication;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Managers.Interfaces;
using Common.DTO.Sockets;
using Microsoft.EntityFrameworkCore;
using Common.Entities;

namespace Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly MagnisChatContext _context;

        public AccountManager(MagnisChatContext context)
        {
            _context = context;
        }

        public TokenResponse GetToken(ClaimsIdentity identity)
        {           
            var response = new ResponseDTO<TokenResponse>();
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: "MagnisChatServer",
                audience: "ChatAudience",
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(60*24)),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("security_80_lvl.ty_ne_proydesh")), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var tokenResponse = new TokenResponse
            {
                Token = encodedJwt,               
                UserLogin = identity.Claims.Where(c => c.Type == "Login").FirstOrDefault().Value,
                UserId = identity.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value
            };

            return tokenResponse;
        }

        public async Task<ResponseDTO<ClaimsIdentity>> GetIdentityAsync(string login, string password)
        {           
            var response = new ResponseDTO<ClaimsIdentity>();

            var user = await _context.Users.FirstOrDefaultAsync(p => p.Login == login);
            if (user == null)
            {               
                response.Error = new Error(404, "Invalid username");
                return response;
            }
            if (user.Password != password)
            {               
                response.Error = new Error(400, "Invalid Password");
                return response;
            }

            var claims = new List<Claim>
            {
                new Claim("Login", login),                
                new Claim("Id", user.Id.ToString())
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity
                (claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            response.Data = claimsIdentity;
            return response;
        }

        public async Task<ResponseDTO<SocketTokenDTO>> GetSocketTokenAsync(Guid userId)
        {
            var token = new SocketTokenEntity
            {
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.Add(TimeSpan.FromDays(7)),
                Id = Guid.NewGuid()                
            };

            await _context.SocketTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return new ResponseDTO<SocketTokenDTO> { Data = new SocketTokenDTO(token) };
        }

        public async Task<ResponseDTO<UserProfile>> GetUserAsync(Guid userId)
        {
            var response = new ResponseDTO<UserProfile>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null)
            {
                response.Error = new Error(404, "User not found");
                return response;
            }

            response.Data = new UserProfile(user);
            return response;
        }

        public async Task<ResponseDTO<bool>> Register(RegistrationRequest request)
        {
            var response = new ResponseDTO<bool>();

            var isNickTaken = _context.Users.Where(u => u.Login == request.Login).Count();
            if(isNickTaken != 0)
            {
                response.Error = new Error(409, "Nick name is taken");
                return response;
            }

            await _context.Users.AddAsync(new UserEntity(request));
            await _context.SaveChangesAsync();

            response.Data = true;
            return response;
        }
    }
}
