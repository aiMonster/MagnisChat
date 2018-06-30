using Common.DTO.Account;
using Common.DTO.Communication;
using Common.DTO.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Managers.Interfaces
{
    public interface IAccountManager
    {
        TokenResponse GetToken(ClaimsIdentity identity);
        Task<ResponseDTO<ClaimsIdentity>> GetIdentityAsync(string login, string password);
        Task<ResponseDTO<SocketTokenDTO>> GetSocketTokenAsync(Guid userId);
        Task<ResponseDTO<UserProfile>> GetUserAsync(Guid userId);
    }
}
