using Common.DTO.Account;
using Common.DTO.Sockets;
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

        public List<Guid> Participants { get; set; }
        public Guid AdminId { get; set; }        
    }
}
