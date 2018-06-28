using Common.DTO.Account;
using Common.DTO.Messages;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class MagnisChatContext
    {
        public List<UserDTO> Users = new List<UserDTO>
        {
            new UserDTO("vova", "1234", new Guid("14956c41-22cc-46ee-9425-560e770afb6e")),
            new UserDTO("homa", "qwerty", new Guid("7783de4e-c572-4bd3-8bea-e01d2c95bd83")),
            new UserDTO("artur", "pirozok", new Guid("81d17291-a08d-40c1-b19a-711fbc0977b8"))
        };

        public List<SocketTokenDTO> SocketTokens = new List<SocketTokenDTO>();
        public List<RoomDTO> Rooms = new List<RoomDTO>();
        public List<MessageDTO> Messages = new List<MessageDTO>();
        public List<FileDTO> Files = new List<FileDTO>();
    }
}
