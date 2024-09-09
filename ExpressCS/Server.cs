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

                if (req.IsWebSocketRequest)
                {
                    WebSocketRouteStruct? foundWebSocketRoute = null;
                    foreach (WebSocketRouteStruct route in StorageUtil.WebSocketRoutes)
                    {
                        if(route.Path.Contains(":"))
                        {
                            string[] routePath = route.Path.Split('/');
                            string[] reqPath = req.Url.AbsolutePath.Split('/');

                            if (routePath.Length != reqPath.Length)
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

                            if (match)
                            {
                                foundWebSocketRoute = route;
                                break;
                            }
                        }

                        if (route.Path == req.Url.AbsolutePath)
                        {
                            foundWebSocketRoute = route;
                        }
                    }

                    if (foundWebSocketRoute != null)
                    {
                        WebSocketHandler.HandleSocketIitialization(ctx, foundWebSocketRoute.Value);
                        continue;
                    }

                    continue;
                }

                //ERROR: System.Net.ProtocolViolationException: 'Bytes to be written to the stream exceed the Content-Length bytes size specified.'
                if (req.HttpMethod == "HEAD")
                {
                    Console.WriteLine("HEAD request");
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
                    Headers = req.Headers,
                    DynamicParams = null
                };

                RouteStruct? foundRoute = null;
                foreach (RouteStruct route in StorageUtil.Routes)
                {
                    string reqURL = req.Url.AbsolutePath.EndsWith("/") ? req.Url.AbsolutePath.Remove(req.Url.AbsolutePath.Length - 1) : req.Url.AbsolutePath;
                    if (route.Path.Contains(":"))
                    {
                        string[] routePath = route.Path.Split('/');
                        string[] reqPath = reqURL.Split('/');

                        if (routePath.Length != reqPath.Length)
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
                    if (route.Path == reqURL && (route.Methods.Contains(requstMethode) || route.Methods.Contains(Struct.HttpMethod.ANY)))
                    {
                        foundRoute = route;
                    }
                }

                if (foundRoute == null) {
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

                    if (StorageUtil.CustomError != null)
                    {
                        RouteStruct.Response routeResponse = new RouteStruct.Response();
                        await StorageUtil.CustomError.Value.Callback(parsedRequest, routeResponse);

                        await SendMethodes.handleResponse(resp, routeResponse);
                        continue;
                    }

                    await SendMethodes.handleResponse(resp, getErrorResponse());
                    continue;
                }

                parsedRequest.DynamicParams = HelperUtil.getDynamicParamsFromURL(foundRoute.Value.Path, req.Url.AbsolutePath);


                if (StorageUtil.Middleware != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await StorageUtil.Middleware.Value.Callback(parsedRequest, routeResponse);
                    if(routeResponse.Data != null)
                    {
                        await SendMethodes.handleResponse(resp, routeResponse);
                        continue;
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
                await SendMethodes.handleResponse(resp, getErrorResponse());
            }
        }

        private static RouteStruct.Response getErrorResponse()
        {
            return new RouteStruct.Response
            {
                Data = "<html><body><h1>404 Not Found</h1></body></html>",
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8,
                ContentLength64 = Encoding.UTF8.GetByteCount("<html><body><h1>404 Not Found</h1></body></html>")
            };
        }
    }
}
