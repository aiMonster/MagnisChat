using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class RoomParticipants
    {
        public Guid RoomId { get; set; }
        public RoomEntity Room { get; set; }

        public Guid UserId { get; set; }
        public UserEntity User { get; set; }

        public RoomParticipants()
        {

        }
    }
}
