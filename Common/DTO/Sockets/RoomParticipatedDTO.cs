using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Sockets
{
    public class RoomParticipatedDTO
    {
        public Guid RoomId { get; set; }
        public Guid UserId { get; set; }
    }
}
