using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Sockets
{
    public class FileStatusDTO
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public int PartsUploaded { get; set; }
    }
}
