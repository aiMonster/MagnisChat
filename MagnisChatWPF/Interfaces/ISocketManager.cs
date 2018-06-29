using Common.DTO.Messages;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Interfaces
{
    public delegate void SocketMessage<T>(SocketResponseDTO<T> response);

    public interface ISocketManager
    {
        void Login(Guid token);
        void Connect();

        event Action Authorized;
        event SocketMessage<RoomDTO> RoomCreated;
        event SocketMessage<MessageDTO> NewMessageReceived;        
        event SocketMessage<RoomParticipatedDTO> RoomParticipated;
        event SocketMessage<RoomParticipatedDTO> RoomLeft;
        event SocketMessage<FileStatusDTO> FileStatusChanged;
    }
}
