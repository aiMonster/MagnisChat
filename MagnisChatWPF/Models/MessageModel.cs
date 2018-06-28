using Common.DTO.Account;
using Common.DTO.Messages;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Models
{
    public class MessageModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public string content { get; set; }
        public string  SenderLogin { get; set; }
        public string SenderId { get; set; }
        public DateTime SendingTime { get; set; }
        public MessageTypes Type { get; set; }
        public Guid FileId { get; set; }

        public MessageModel()
        {

        }

        public MessageModel(MessageDTO message, UserProfile user)
        {
            Id = message.Id;
            content = message.Content;
            SenderLogin = user.Login;
            SendingTime = message.SendingTime;
            Type = message.Type;
            SenderId = user.Id.ToString();
        }

        public string Content
        {

            get { return content; }
            set
            {
                if (content == value)
                    return;
                content = value;
                SendPropertyChanged("Content");
            }
        }


        private void SendPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
