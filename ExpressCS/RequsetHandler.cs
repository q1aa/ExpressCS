using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ExpressCS.Struct.WebSocketRouteStruct;
using ExpressCS.Struct;
using ExpressCS.Utils;

namespace ExpressCS
{
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

            await HandleWebSocketConnection(webSocket, route, req, route.MessageBytes);
        }
        private static async Task HandleWebSocketConnection(WebSocket webSocket, WebSocketRouteStruct route, HttpListenerRequest req, int messageBytes)
        {
            byte[] buffer = new byte[messageBytes];
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    DownloadSizeUtil.AddWebSocketDownloadSize(result.Count);

                    WebSocketRequest request = new WebSocketRequest
                    {
                        Url = req.Url.AbsolutePath,
                        Host = req.UserHostAddress,
                        Headers = req.Headers,
                        Data = message,
                        DynamicParams = HelperUtil.getDynamicParamsFromURL(route.Path, req.Url.AbsolutePath)
                    };

                    WebSocketResponse response = new WebSocketResponse
                    {
                        Headers = new List<string>()
                    };

                    await route.Callback(request, response);

                    if (response.Data != null)
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response.Data);
                        UploadSizeUtil.AddWebSocketUploadSize(responseBytes.Length);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"{e.Message}  -  Url: {req.Url.AbsolutePath} - @{DateTime.Now}");
                }
            }
        }
    }
}
