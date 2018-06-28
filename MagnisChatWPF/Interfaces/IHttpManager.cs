using Common.DTO.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Interfaces
{
    public interface IHttpManager
    {
        Task<ResponseDTO<T>> GetAsync<T>(string route);
        Task<ResponseDTO<T>> PutAsync<T>(string path);
        Task<ResponseDTO<T>> PostAsync<T>(string path, object body = null);
        void Authorize(string token);
    }
}
