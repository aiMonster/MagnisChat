using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Sockets
{
    public class SocketLoginRequest : BaseMessage
    {
        public Guid Token { get; set; }
    }
}
