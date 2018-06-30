using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public List<SocketTokenEntity> SocketTokens { get; set; }
        public List<RoomParticipants> Rooms { get; set; }

        public UserEntity()
        {

        }

    }
}
