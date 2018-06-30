using Common.DTO.Account;
using Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Sockets
{
    public class SocketTokenDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime ExpirationDate { get; set; }

        public SocketTokenDTO() { }

        public SocketTokenDTO(SocketTokenEntity entity)
        {
            Id = entity.Id;
            UserId = entity.UserId;
            ExpirationDate = entity.ExpirationDate;
        }
    }
}
