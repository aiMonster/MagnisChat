using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Messages
{
    public class FilePartDTO
    {       
        public int PartNumber { get; set; }
        public byte[] Content { get; set; }
    }
}
