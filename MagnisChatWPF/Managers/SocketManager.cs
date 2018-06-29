using Common.DTO.Messages;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using Common.Enums;
using MagnisChatWPF.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebSocketSharp;

namespace MagnisChatWPF.Managers
{
    [Export(typeof(ISocketManager))]
    public class SocketManager : ISocketManager
    {
        private WebSocket _webSocket;
        private readonly string _host;       

        public event Action Authorized;
        public event Action<string> Failed;
        public event SocketMessage<RoomDTO> RoomCreated;
        public event SocketMessage<MessageDTO> NewMessageReceived;       
        public event SocketMessage<RoomParticipatedDTO> RoomParticipated;
        public event SocketMessage<RoomParticipatedDTO> RoomLeft;
        public event SocketMessage<FileStatusDTO> FileStatusChanged;
        
        public SocketManager()
        {                    
            _host = Properties.Settings.Default["WebSocketApi"].ToString();
        }

        public void Connect()
        {
            if(_webSocket != null)
            {
                Unsubscribe();
                _webSocket.Close();               
            }
            _webSocket = new WebSocket(_host);
            _webSocket.OnMessage += (sender, e) => HandleSocketMessage(sender, e);

            try
            {
                _webSocket.Connect();
            }
            catch(Exception ex)
            {
                Failed?.Invoke(ex.Message);
            }
            
        }

        public void Unsubscribe()
        {
            foreach (var a in Authorized.GetInvocationList())
            {
                Authorized -= (Action)a;
            }

            foreach (var a in RoomCreated.GetInvocationList())
            {
                RoomCreated -= (SocketMessage<RoomDTO>)a;
            }

            foreach(var a in NewMessageReceived.GetInvocationList())
            {
                NewMessageReceived -= (SocketMessage<MessageDTO>)a;
            }

            foreach(var a in RoomParticipated.GetInvocationList())
            {
                RoomParticipated -= (SocketMessage<RoomParticipatedDTO>)a;
            }

            foreach (var a in RoomLeft.GetInvocationList())
            {
                RoomLeft -= (SocketMessage<RoomParticipatedDTO>)a;
            }

            foreach (var a in FileStatusChanged.GetInvocationList())
            {
                FileStatusChanged -= (SocketMessage<FileStatusDTO>)a;
            }
        }

        public void Login(Guid token)
        {
            var loginRequest = new SocketLoginRequest { Token = token, Type = SocketMessageTypes.LogInRequest }; 
            try
            {
                _webSocket.Send(JsonConvert.SerializeObject(loginRequest));
            }
            catch(Exception ex)
            {
                Failed(ex.Message);
            }    
        }

        private void HandleSocketMessage(object sender, MessageEventArgs e)
        {
            var message = e.Data;
            try
            {
                var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(message);
                if (baseMessage.Type == SocketMessageTypes.SuccessfullyAuthorized)
                {                    
                    Authorized?.Invoke();
                }
                else if (baseMessage.Type == SocketMessageTypes.RoomCreated)
                {
                    var response = JsonConvert.DeserializeObject<SocketResponseDTO<RoomDTO>>(message);
                    RoomCreated?.Invoke(response);
                }
                else if (baseMessage.Type == SocketMessageTypes.NewMessage)
                {
                    var response = JsonConvert.DeserializeObject<SocketResponseDTO<MessageDTO>>(message);
                    NewMessageReceived?.Invoke(response);
                }                
                else if (baseMessage.Type == SocketMessageTypes.RoomParticipated)
                {
                    var response = JsonConvert.DeserializeObject<SocketResponseDTO<RoomParticipatedDTO>>(message);
                    RoomParticipated?.Invoke(response);
                }
                else if(baseMessage.Type == SocketMessageTypes.RoomLeft)
                {
                    var response = JsonConvert.DeserializeObject<SocketResponseDTO<RoomParticipatedDTO>>(message);
                    RoomLeft?.Invoke(response);
                }
                else if(baseMessage.Type == SocketMessageTypes.FileStatusChanged)
                {
                    var response = JsonConvert.DeserializeObject<SocketResponseDTO<FileStatusDTO>>(message);
                    FileStatusChanged?.Invoke(response);
                }
            }
            catch (Exception ex)
            {
                Failed?.Invoke(ex.Message);
            }
        }
    }
}
