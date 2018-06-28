using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Managers.WebSockets
{
    public abstract class BaseWebSocketHandler
    {
        public WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        public BaseWebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
        }


        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;
            try
            {
                await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                 offset: 0,
                                                                 count: message.Length),
                                  messageType: WebSocketMessageType.Text,
                                  endOfMessage: true,
                                  cancellationToken: CancellationToken.None);
            }
            catch { }
           
        }

        public async Task SendMessageAsync(WebSocket socket, object obj)
        {
            var str = JsonConvert.SerializeObject(obj);
            await SendMessageAsync(socket, str);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public async Task SendMessageToAllAsync(object obj)
        {
            await SendMessageToAllAsync(JsonConvert.SerializeObject(obj));
        }

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
