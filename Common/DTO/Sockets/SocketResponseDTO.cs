using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Sockets
{
    public class SocketResponseDTO<T> : BaseMessage
    {
        public T Model { get; set; }
    }
}
