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
        Task<ResponseDTO<IEnumerable<RoomDTO>>> GetAllRoomsAsync();
        Task<ResponseDTO<IEnumerable<RoomDTO>>> GetUserRoomsAsync(Guid userId);
        Task<ResponseDTO<IEnumerable<RoomDTO>>> GetOtherRoomsAsync(Guid userId);
        Task<ResponseDTO<bool>> ParticipateRoomAsync(Guid roomId, Guid userId);
        Task<ResponseDTO<bool>> LeaveRoomAsync(Guid roomId, Guid userId);
    }
}
