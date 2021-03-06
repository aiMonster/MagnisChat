﻿using Common.Entities;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Messages
{
    public class MessageDTO
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RoomId { get; set; }

        public MessageTypes Type { get; set; }
        public string Content { get; set; }
        public DateTime SendingTime { get; set; }

        public MessageDTO() { }

        public MessageDTO(MessageEntity entity)
        {
            Id = entity.Id;
            SenderId = entity.SenderId;
            RoomId = entity.RoomId;

            Type = entity.Type;
            Content = entity.Content;
            SendingTime = entity.SendingTime;
        }
    }
}
