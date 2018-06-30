using Common.DTO.Communication;
using Common.DTO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers.Interfaces
{
    public interface IFileManager
    {
        Task<ResponseDTO<FileDTO>> GetFileAsync(Guid fileId);
        Task<ResponseDTO<bool>> UploadPartFileAsync(FilePartDTO file, Guid fileId);
        Task<ResponseDTO<FilePartDTO>> DownloadPartFileAsync(Guid fileId, int partNumber);
    }
}
