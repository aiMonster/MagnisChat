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
        ResponseDTO<ClaimsIdentity> GetIdentity(string login, string password);
        ResponseDTO<SocketTokenDTO> GetSocketToken(Guid userId);
        ResponseDTO<UserProfile> GetUser(Guid userId);
    }
}
