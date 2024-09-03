using ExpressCS.Struct;
using ExpressCS.Types;
using ExpressCS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ExpressCS.Utils;
using Microsoft.AspNetCore.Server.HttpSys;
using static ExpressCS.Struct.WebSocketRouteStruct;
using System.Net.WebSockets;
using System.Collections.Specialized;


namespace ExpressCS
{
    internal class Server
    {
        public static async Task HandleIncomeRequests()
        {
            while (true)
            {
                HttpListenerContext ctx = await StorageUtil.Listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if(req.IsWebSocketRequest)
                {
                    foreach (WebSocketRouteStruct route in StorageUtil.WebSocketRoutes)
                    {
                        if (req.Url.AbsolutePath == route.Path)
                        {
                            HttpListenerWebSocketContext webSocketContext = await ctx.AcceptWebSocketAsync(null);
                            WebSocket webSocket = webSocketContext.WebSocket;

                            if (route.ConnectionEstablished != null)
                            {
                                WebSocketRequest request = new WebSocketRequest
                                {
                                    Url = webSocket.ToString(),
                                    Host = webSocket.ToString(),
                                    Headers = new NameValueCollection(),
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
                    }

                    continue;
                }

                if (req.HttpMethod == "HEAD")
                {
                    resp.Close();
                    continue;
                }

                string rawBody = req.HasEntityBody ? new StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd() : null;
                RouteStruct.Request parsedRequest = new RouteStruct.Request()
                {
                    Url = req.Url.AbsolutePath,
                    Method = req.HttpMethod,
                    Host = req.UserHostName,
                    UserAgent = req.UserAgent,
                    Body = rawBody,
                    JSONBody = HelperUtil.parseJSONBody(rawBody),
                    QueryParams = HelperUtil.getQueryParamsFromURL(req.RawUrl),
                    ContentType = req.ContentType,
                    Headers = req.Headers
                };

                if (StorageUtil.Middleware != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await StorageUtil.Middleware.Value.Callback(parsedRequest, routeResponse);

                    if (routeResponse.Data != null)
                    {
                        await SendMethodes.handleResponse(resp, routeResponse);
                        continue;
                    }
                }

                RouteStruct? foundRoute = null;
                foreach (RouteStruct route in StorageUtil.Routes)
                {
                    if(route.Path.Contains(":"))
                    {
                        string[] routePath = route.Path.Split('/');
                        string[] reqPath = req.Url.AbsolutePath.Split('/');

                        if(routePath.Length != reqPath.Length)
                        {
                            continue;
                        }

                        bool match = true;
                        for (int i = 0; i < routePath.Length; i++)
                        {
                            if (routePath[i].StartsWith(":"))
                            {
                                continue;
                            }

                            if (routePath[i] != reqPath[i])
                            {
                                match = false;
                                break;
                            }
                        }

                        if(match)
                        {
                            foundRoute = route;
                            break;
                        }
                    }


                    Struct.HttpMethod requstMethode = HelperUtil.convertRequestMethode(req.HttpMethod);
                    if (route.Path == req.Url.AbsolutePath && (route.Methods.Contains(requstMethode) || route.Methods.Contains(Struct.HttpMethod.ANY)))
                    {
                        foundRoute = route;
                    }
                }

                if (foundRoute != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await foundRoute.Value.Callback(parsedRequest, routeResponse);
                    await SendMethodes.handleResponse(resp, routeResponse);
                    continue;
                }

                bool staticFileFound = false;
                foreach (StaticFileStruct staticFile in StorageUtil.StaticFiles)
                {
                    if (req.Url.AbsolutePath.StartsWith(staticFile.WebPath))
                    {
                        string filePath = req.Url.AbsolutePath.Replace(staticFile.WebPath, "");
                        if (!File.Exists(staticFile.DirectoryPath.FullName + "/" + filePath))
                        {
                            continue;
                        }

                        staticFileFound = true;
                        await SendMethodes.handleResponse(resp, new RouteStruct.Response
                        {
                            ResponseType = ResponseType.SENDFILE,
                            Data = staticFile.DirectoryPath.FullName + "/" + filePath
                        });
                    }
                }
                if (staticFileFound) continue;


                if (StorageUtil.CustomError != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await StorageUtil.CustomError.Value.Callback(parsedRequest, routeResponse);

                    await SendMethodes.handleResponse(resp, routeResponse);
                    continue;
                }
                await SendMethodes.handleResponse(resp, new RouteStruct.Response
                {
                    Data = "<html><body><h1>404 Not Found</h1></body></html>",
                    ContentType = "text/html",
                    ContentEncoding = Encoding.UTF8,
                    ContentLength64 = Encoding.UTF8.GetByteCount("<html><body><h1>404 Not Found</h1></body></html>")
                });
            }
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
