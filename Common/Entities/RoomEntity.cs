using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class RoomEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public List<RoomParticipants> Participants { get; set; }
        public List<MessageEntity> Messages { get; set; }
        public Guid AdminId { get; set; }

        public RoomEntity()
        {

        }
    }
}
