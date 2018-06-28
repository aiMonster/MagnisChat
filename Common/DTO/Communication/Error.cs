using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Communication
{
    public class Error
    {

        public int ErrorCode { get; set; }
        public string ErrorDescription { get; set; }

        public Error() { }

        public Error(int errorCode, string description)
        {
            ErrorDescription = description;
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            return $"Status code: {ErrorCode},\nDescription: {ErrorDescription}\n";
        }
    }
}
