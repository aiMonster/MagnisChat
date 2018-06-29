using Common.DTO.Communication;
using Common.DTO.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers.Interfaces
{
    public interface IRoomsManager
    {
        Task<ResponseDTO<Guid>> CreateRoomAsync(RoomRequest request, Guid userId);
        ResponseDTO<IEnumerable<RoomDTO>> GetAllRooms();
        ResponseDTO<IEnumerable<RoomDTO>> GetUserRooms(Guid userId);
        ResponseDTO<IEnumerable<RoomDTO>> GetOtherRooms(Guid userId);
        Task<ResponseDTO<bool>> ParticipateRoomAsync(Guid roomId, Guid userId);
        Task<ResponseDTO<bool>> LeaveRoomAsync(Guid roomId, Guid userId);
    }
}
