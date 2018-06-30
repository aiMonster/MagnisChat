using Common.DTO.Account;
using Common.DTO.Sockets;
using Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rooms
{
    public class RoomDTO 
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public List<Guid> Participants { get; set; } = new List<Guid>();
        public Guid AdminId { get; set; }   
        
        public RoomDTO() { }
        public RoomDTO(RoomEntity entity)
        {
            Id = entity.Id;
            Title = entity.Title;
            AdminId = entity.AdminId;
            Participants = entity.Participants.Select(p => p.UserId).ToList();
        }
    }
}
