using Common.DTO.Sockets;
using Common.Enums;
using DataAccessLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Managers.WebSockets
{
    public class ChatHandler : BaseWebSocketHandler
    {       
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _userSessions;
        private readonly MagnisChatContext _context;

        public ChatHandler(WebSocketConnectionManager webSocketConnectionManager, MagnisChatContext context) : base(webSocketConnectionManager)
        {           
            _userSessions = new ConcurrentDictionary<Guid, HashSet<string>>();
            _context = context;
        }       

        public override Task OnDisconnected(WebSocket socket)
        {           

            FindAndRemoveSession(socket);
            return base.OnDisconnected(socket);
        }

        public async Task SendMessageByUserId(Guid userId, object message)
        {            
            if (_userSessions.ContainsKey(userId))
            {
                var jsonMessage = JsonConvert.SerializeObject(message);
                foreach (var socketId in _userSessions.Where(s => s.Key == userId).FirstOrDefault().Value)
                {
                    await SendMessageAsync(socketId, jsonMessage);
                }
            }
        }

        public async Task SendMessageByUserId(IEnumerable<Guid> usersId, object message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            foreach(var userId in usersId)
            {
                if(_userSessions.ContainsKey(userId))
                {
                    foreach (var socketId in _userSessions.Where(s => s.Key == userId).FirstOrDefault().Value)
                    {
                        await SendMessageAsync(socketId, jsonMessage);
                    }
                }
            }
        }

        public async Task SendForAllAuthorizedUsers(object message)
        {
            foreach(var user in _userSessions)
            {
                await SendMessageByUserId(user.Key, message);
            }
        }


        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);           
            try
            {
                var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(message);
                if (baseMessage == null)
                {
                    await SendMessageAsync(socket, new BaseMessage { Type = SocketMessageTypes.UnknownRequest });
                    return;
                }

                if (baseMessage.Type == SocketMessageTypes.LogInRequest)
                {
                    var request = JsonConvert.DeserializeObject<SocketLoginRequest>(message);
                    AddSession(socket, request);
                    return;
                }

                if (!IsAuthorized(socket))
                {
                    await SendMessageAsync(socket, new BaseMessage { Type = SocketMessageTypes.UnAuthorized });
                    return;
                }
            }   
            catch(Exception ex)
            {
                await SendMessageAsync(socket, new BaseMessage { Type = SocketMessageTypes.UnhandledException });
            }
        }

        private bool IsAuthorized(WebSocket socket)
        {
            return _userSessions.Values.Where(x => x.Contains(WebSocketConnectionManager.GetId(socket))).Any();
            //foreach(var item in _userSessions)
            //{
            //    foreach(var session in item.Value)
            //    {
            //        if(session == WebSocketConnectionManager.GetId(socket))
            //        {
            //            return true;
            //        }
            //    }
            //}
            //return false;
        }

        private async void AddSession(WebSocket socket, SocketLoginRequest request)
        {
            var token = _context.SocketTokens.Where(st => st.Id == request.Token).FirstOrDefault();
            if(token == null || token.ExpirationDate <= DateTime.UtcNow)
            {                
                await SendMessageAsync(socket, new BaseMessage { Type = SocketMessageTypes.TokenNotValid });
                return;
            }           

            if (!_userSessions.ContainsKey(token.UserId))
            {
                _userSessions.TryAdd(token.UserId, new HashSet<string>());
            }
            _userSessions[token.UserId].Add(WebSocketConnectionManager.GetId(socket));

            await SendMessageAsync(socket, new BaseMessage { Type = SocketMessageTypes.SuccessfullyAuthorized });
        }

        private void FindAndRemoveSession(WebSocket socket)
        {
            var toRemove = new List<Guid>();
            foreach (var item in _userSessions)
            {
                item.Value.Remove(WebSocketConnectionManager.GetId(socket));
                if (item.Value.Count == 0)
                {
                    toRemove.Add(item.Key);
                }
            }
            foreach (var item in toRemove)
            {
                _userSessions.TryRemove(item, out var removed);
            }
        }
    }
}
