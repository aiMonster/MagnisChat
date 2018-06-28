using Common.DTO.Communication;
using Common.DTO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers.Interfaces
{
    public interface IMessageManager
    {        
        Task<ResponseDTO<FileDTO>> SendFileMessageAsync(FileMessageRequest request, Guid roomId, Guid userId);
        Task<ResponseDTO<bool>> SendTextMessageAsync(TextMessageRequest request, Guid roomId, Guid userId);
        ResponseDTO<IEnumerable<MessageDTO>> GetMessages(Guid roomId);
               
    }
}
