using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ExpressCS.Struct.WebSocketRouteStruct;
using ExpressCS.Struct;

namespace ExpressCS
{
    class HttpRequestHandler
    {

    }

    public class WebSocketHandler
    {
        public static async Task HandleSocketIitialization(HttpListenerContext ctx, WebSocketRouteStruct route)
        {
            HttpListenerRequest req = ctx.Request;
            HttpListenerWebSocketContext webSocketContext = await ctx.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            if (route.ConnectionEstablished != null)
            {
                WebSocketRequest request = new WebSocketRequest
                {
                    Url = req.Url.AbsolutePath,
                    Host = req.UserHostAddress,
                    Headers = req.Headers,
                    Data = null
                };

                WebSocketResponse response = new WebSocketResponse
                {
                    Headers = new List<string>(),
                };

                await route.ConnectionEstablished(request, response);

                if (response.Data != null)
                {
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response.Data);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }

            await HandleWebSocketConnection(webSocket, route.Callback, req);
        }
        private static async Task HandleWebSocketConnection(WebSocket webSocket, Func<WebSocketRequest, WebSocketResponse, Task> callback, HttpListenerRequest req)
        {
            byte[] buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                WebSocketRequest request = new WebSocketRequest
                {
                    Url = req.Url.AbsolutePath,
                    Host = req.UserHostAddress,
                    Headers = req.Headers,
                    Data = message
                };

                WebSocketResponse response = new WebSocketResponse
                {
                    Headers = new List<string>()
                };

                await callback(request, response);

                if (response.Data != null)
                {
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response.Data);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
