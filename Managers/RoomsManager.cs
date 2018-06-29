using Common.DTO.Communication;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using Common.Enums;
using DataAccessLayer;
using Managers.Interfaces;
using Managers.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers
{
    public class RoomsManager : IRoomsManager
    {
        private readonly MagnisChatContext _context;
        private readonly ChatHandler _chatHandler;

        public RoomsManager(MagnisChatContext context, ChatHandler chatHandler)
        {
            _context = context;
            _chatHandler = chatHandler;
        }

        public ResponseDTO<IEnumerable<RoomDTO>> GetAllRooms()
        {
            return new ResponseDTO<IEnumerable<RoomDTO>>
                {
                    Data = _context.Rooms
                };
        }

        public ResponseDTO<IEnumerable<RoomDTO>> GetUserRooms(Guid userId)
        {
            return new ResponseDTO<IEnumerable<RoomDTO>>
                {
                    Data = _context.Rooms.Where(r => r.Participants.Contains(userId))
                };
        }

        public ResponseDTO<IEnumerable<RoomDTO>> GetOtherRooms(Guid userId)
        {
            return new ResponseDTO<IEnumerable<RoomDTO>>
                {
                    Data = _context.Rooms.Except(_context.Rooms.Where(r => r.Participants.Contains(userId)))
                };
        }


        public async Task<ResponseDTO<Guid>> CreateRoomAsync(RoomRequest request, Guid userId)
        {
            var response = new ResponseDTO<Guid>();
            var room = new RoomDTO
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                AdminId = userId,
                Participants = new List<Guid>() { userId }
            };
            _context.Rooms.Add(room);

            await _chatHandler.SendForAllAuthorizedUsers(
                new SocketResponseDTO<RoomDTO>
                {
                    Type = SocketMessageTypes.RoomCreated, Model = room
                });

            response.Data = room.Id;
            return response;
        }       

        public async Task<ResponseDTO<bool>> ParticipateRoomAsync(Guid roomId, Guid userId)
        {
            var response = new ResponseDTO<bool>();
            var room = _context.Rooms.Where(r => r.Id == roomId).FirstOrDefault();
            if(room == null)
            {
                response.Error = new Error(404, "Room not found");
                return response;
            }
            
            if(room.Participants.Contains(userId))
            {
                response.Error = new Error(409, "You already participate");
                return response;
            }

            room.Participants.Add(userId);
            await _chatHandler.SendForAllAuthorizedUsers(
                new SocketResponseDTO<RoomParticipatedDTO>
                {
                    Model = new RoomParticipatedDTO { RoomId = roomId, UserId = userId },
                    Type = SocketMessageTypes.RoomParticipated
                });

            response.Data = true;
            return response;
        }

        public async Task<ResponseDTO<bool>> LeaveRoomAsync(Guid roomId, Guid userId)
        {
            var response = new ResponseDTO<bool>();
            var room = _context.Rooms.Where(r => r.Id == roomId).FirstOrDefault();
            if (room == null)
            {
                response.Error = new Error(404, "Room not found");
                return response;
            }

            if (!room.Participants.Contains(userId))
            {
                response.Error = new Error(409, "You are not participating");
                return response;
            }

            room.Participants.Remove(userId);
            await _chatHandler.SendForAllAuthorizedUsers(
                new SocketResponseDTO<RoomParticipatedDTO>
                {
                    Model = new RoomParticipatedDTO { RoomId = roomId, UserId = userId },
                    Type = SocketMessageTypes.RoomLeft
                });

            response.Data = true;
            return response;
        }


    }
}
