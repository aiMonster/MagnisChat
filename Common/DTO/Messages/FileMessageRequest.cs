using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Messages
{
    public class FileMessageRequest
    {      
        public int PartSize { get; set; }
        public long Size { get; set; }
        public int Parts { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
