using Common.DTO.Communication;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using Common.Entities;
using Common.Enums;
using DataAccessLayer;
using Managers.Interfaces;
using Managers.WebSockets;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ResponseDTO<IEnumerable<RoomDTO>>> GetAllRoomsAsync()
        {
            return new ResponseDTO<IEnumerable<RoomDTO>>
                {
                    Data = await _context.Rooms.Select(r => new RoomDTO(r)).ToListAsync()
                };
        }

        public async Task<ResponseDTO<IEnumerable<RoomDTO>>> GetUserRoomsAsync(Guid userId)
        {
            return new ResponseDTO<IEnumerable<RoomDTO>>
                {
                    Data = await  _context.Rooms.Where(r => r.Participants
                                                             .Select(rr => rr.UserId)
                                                             .Contains(userId))
                                                             .Include(r => r.Participants)
                                                .Select(r => new RoomDTO(r)).ToListAsync()
                };
        }

        public async Task<ResponseDTO<IEnumerable<RoomDTO>>> GetOtherRoomsAsync(Guid userId)
        {           
            return new ResponseDTO<IEnumerable<RoomDTO>>
            {
                Data = await _context.Rooms.Where(r => !r.Participants.Select(rr => rr.UserId).Contains(userId))
                                                .Include(r => r.Participants)
                                                .Select(r => new RoomDTO(r)).ToListAsync()
            };
        }


        public async Task<ResponseDTO<Guid>> CreateRoomAsync(RoomRequest request, Guid userId)
        {
            var response = new ResponseDTO<Guid>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var room = new RoomEntity
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                AdminId = userId,
                Participants = new List<RoomParticipants>()
            };
            room.Participants.Add(new RoomParticipants { RoomId = room.Id, UserId = userId });

            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();

            await _chatHandler.SendForAllAuthorizedUsers(
                new SocketResponseDTO<RoomDTO>
                {
                    Type = SocketMessageTypes.RoomCreated, Model = new RoomDTO(room)
                });

            response.Data = room.Id;
            return response;
        }       

        public async Task<ResponseDTO<bool>> ParticipateRoomAsync(Guid roomId, Guid userId)
        {
            var response = new ResponseDTO<bool>();
            var participants = await _context.Rooms.Where(r => r.Id == roomId).SelectMany(p => p.Participants).Select(rp => rp.User).ToListAsync();
            if(participants == null)
            {
                response.Error = new Error(404, "Room not found");
                return response;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(participants.Contains(user))
            {
                response.Error = new Error(409, "You already participate");
                return response;
            }
            
            _context.Add(new RoomParticipants { RoomId = roomId, UserId = userId});
            await _context.SaveChangesAsync();
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
            var participants = await _context.Rooms.Where(r => r.Id == roomId).SelectMany(p => p.Participants).Select(rp => rp.User).ToListAsync();
            if (participants == null)
            {
                response.Error = new Error(404, "Room not found");
                return response;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (!participants.Contains(user))
            {
                response.Error = new Error(409, "You are not participating");
                return response;
            }

            var room = await _context.Rooms.Include(r => r.Participants).FirstOrDefaultAsync(r => r.Id == roomId);
            var toRemove = room.Participants.FirstOrDefault(p => p.UserId == userId);

            _context.Remove(toRemove);
            await _context.SaveChangesAsync();
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
