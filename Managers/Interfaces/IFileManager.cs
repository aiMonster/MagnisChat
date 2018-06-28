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
        ResponseDTO<FileDTO> GetFile(Guid fileId);
        Task<ResponseDTO<bool>> UploadPartFile(FilePartDTO file, Guid fileId);
        Task<ResponseDTO<FilePartDTO>> DownloadPartFile(Guid fileId, int partNumber);
    }
}
