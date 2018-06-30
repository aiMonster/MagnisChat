using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class MessageEntity
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }

        public Guid RoomId { get; set; }
        public RoomEntity Room { get; set; }

        public MessageTypes Type { get; set; }
        public string Content { get; set; }
        public DateTime SendingTime { get; set; }

        public MessageEntity()
        {

        }
    }
}
