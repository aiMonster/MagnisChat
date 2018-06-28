﻿using Common.DTO.Communication;
using Common.DTO.Messages;
using Common.DTO.Sockets;
using Common.Enums;
using DataAccessLayer;
using Managers.Interfaces;
using Managers.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers
{
    public class MessageManager : IMessageManager
    {
        private readonly MagnisChatContext _context;
        private readonly ChatHandler _chatHandler;

        public MessageManager(MagnisChatContext context, ChatHandler chatHandler)
        {
            _context = context;
            _chatHandler = chatHandler;
        }       

        public async Task<ResponseDTO<FileDTO>> SendFileMessageAsync(FileMessageRequest request, Guid roomId, Guid userId)
        {
            var response = new ResponseDTO<FileDTO>();
            var room = _context.Rooms.Where(r => r.Id == roomId).FirstOrDefault();
            if (room == null)
            {
                response.Error = new Error(404, "Room not found");
                return response;
            }
            if (!room.Participants.Contains(userId))
            {
                response.Error = new Error(409, "You can't send message here, because you are not participant");
                return response;
            }

            var fileId = Guid.NewGuid();
            var message = new MessageDTO()
            {
                Id = Guid.NewGuid(),
                SenderId = userId,
                RoomId = roomId,
                Content = fileId.ToString(),
                Type = MessageTypes.File,
                SendingTime = DateTime.UtcNow
            };

            var file = new FileDTO()
            {
                Id = fileId,
                MessageId = message.Id,
                
                Name = request.Name,
                Size = request.Size,
                PartSize = request.PartSize,
                Path = request.Path,                
                Parts = request.Parts                 
            };

            _context.Messages.Add(message);
            _context.Files.Add(file);

            var socketDTO = new SocketResponseDTO<MessageDTO>
                {
                    Type = SocketMessageTypes.NewMessage,
                    Model = message
                };
            foreach (var participantId in room.Participants)
            {
                await _chatHandler.SendMessageByUserId(participantId, socketDTO);
            }

            response.Data = file;
            return response;
        }
           
        public async Task<ResponseDTO<bool>> SendTextMessageAsync(TextMessageRequest request, Guid roomId, Guid userId)
        {
            var response = new ResponseDTO<bool>();
            var room = _context.Rooms.Where(r => r.Id == roomId).FirstOrDefault();
            if(room == null)
            {
                response.Error = new Error(404, "Room not found");
                return response;
            }
            if (!room.Participants.Contains(userId))
            {
                response.Error = new Error(409, "You can't send message here, because you are not participant");
                return response;
            }

            var message = new MessageDTO()
            {
                Id = Guid.NewGuid(),
                SenderId = userId,
                RoomId = room.Id,
                Content = request.Content,
                Type = MessageTypes.Text,
                SendingTime = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            var socketDTO = new SocketResponseDTO<MessageDTO>
                {
                    Type = SocketMessageTypes.NewMessage, Model = message
                };

            foreach (var participantId in room.Participants)
            {
                await _chatHandler.SendMessageByUserId(participantId, socketDTO);
            }

            response.Data = true;
            return response; 
        }
        
        public ResponseDTO<IEnumerable<MessageDTO>> GetMessages(Guid roomId)
        {
            return new ResponseDTO<IEnumerable<MessageDTO>>
                {
                    Data = _context.Messages.Where(m => m.RoomId == roomId)
                };
        }       
    }
}
