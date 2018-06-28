using Common.DTO.Account;
using Common.DTO.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Models
{
    public class RoomModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string AdminLogin { get; set; }

        public RoomModel()
        {

        }

        public RoomModel(RoomDTO room, UserProfile user)
        {
            Id = room.Id;
            Title = room.Title;
            AdminLogin = user.Login;
        }
    }
}
