using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Communication
{
    public class ResponseDTO<T>
    {
        public T Data { get; set; }
        public Error Error { get; set; }
    }
}
