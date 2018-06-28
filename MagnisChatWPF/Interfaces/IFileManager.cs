using Common.DTO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Interfaces
{
    public interface IFileManager
    {
        event Action<int> PartDownloaded;
        void Authorize(string token);
        void Upload(FileMessageRequest file, Guid roomId);
        void Download(FileDTO file);
    }
}
