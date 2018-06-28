﻿using Managers.WebSockets;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MagnisChatAPI.Extensions
{
    public class WebSocketMiddleware
    {       
        private readonly RequestDelegate _next;
        private BaseWebSocketHandler _webSocketHandler { get; set; }

        public WebSocketMiddleware(RequestDelegate next, BaseWebSocketHandler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;           
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
           
            await _webSocketHandler.OnConnected(socket);
            await Receive(socket, async (result, buffer) =>
            {
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocketHandler.OnDisconnected(socket);
                    return;
                }

            });

            //TODO - investigate the Kestrel exception thrown when this is the last middleware
            //await _next.Invoke(context);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                            cancellationToken: CancellationToken.None);

                    handleMessage(result, buffer);
                }
            }
            catch
            {
                //When socket was closed enforced
                await _webSocketHandler.OnDisconnected(socket);
            }
           
        }
    }
}