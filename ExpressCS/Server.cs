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
        public static async Task HandleIncomeRequests(bool showTransferedDataSize)
        {
            while (true)
            {
                HttpListenerContext ctx = await StorageUtil.Listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                string requestURL = req.Url.AbsolutePath.ToLower();

                if (showTransferedDataSize) DownloadSizeUtil.AddDownloadSize(req.Headers, req.ContentLength64);

                if (req.IsWebSocketRequest)
                {
                    WebSocketRouteStruct? foundWebSocketRoute = null;
                    foreach (WebSocketRouteStruct route in StorageUtil.WebSocketRoutes)
                    {
                        if (route.Path.Contains(":"))
                        {
                            string[] routePath = route.Path.Split('/');
                            string[] reqPath = requestURL.Split('/');

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

                        if (route.Path == requestURL)
                        {
                            foundWebSocketRoute = route;
                        }
                    }

                    if (foundWebSocketRoute != null)
                    {
                        new Task(async () => await WebSocketHandler.HandleSocketIitialization(ctx, foundWebSocketRoute.Value)).Start();
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

                string rawBody = req.HasEntityBody
                    ? new StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd()
                    : null;
                RouteStruct.Request parsedRequest = new RouteStruct.Request()
                {
                    Url = req.Url.AbsolutePath,
                    Method = req.HttpMethod,
                    Host = req.UserHostName,
                    UserAgent = req.UserAgent,
                    Body = rawBody,
                    JSONBody = HelperUtil.parseJSONBody(rawBody),
                    QueryParams = HelperUtil.getQueryParamsFromURL(req.Url.PathAndQuery),
                    ContentType = req.ContentType,
                    Headers = req.Headers,
                    DynamicParams = null
                };

                RouteStruct? foundRoute = null;
                foreach (RouteStruct route in StorageUtil.Routes)
                {
                    string reqURL = requestURL.EndsWith("/")
                        ? requestURL.Remove(requestURL.Length - 1)
                        : requestURL;
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

                        if (match)
                        {
                            foundRoute = route;
                            break;
                        }
                    }


                    Struct.HttpMethod requstMethode = HelperUtil.convertRequestMethode(req.HttpMethod);
                    if (route.Path == reqURL && (route.Methods.Contains(requstMethode) ||
                                                 route.Methods.Contains(Struct.HttpMethod.ANY)))
                    {
                        foundRoute = route;
                    }
                }

                if (foundRoute == null)
                {
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

                    RouteStruct.Response? errorStaticFileResponse = staticFileExists(requestURL);
                    if (errorStaticFileResponse != null)
                    {
                        await SendMethodes.handleResponse(resp, errorStaticFileResponse);
                        continue;
                    }

                    await SendMethodes.handleResponse(resp, await getErrorResponse(parsedRequest));
                    continue;
                }

                parsedRequest.DynamicParams =
                    HelperUtil.getDynamicParamsFromURL(foundRoute.Value.Path, req.Url.AbsolutePath);


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

                if (foundRoute != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await foundRoute.Value.Callback(parsedRequest, routeResponse);
                    await SendMethodes.handleResponse(resp, routeResponse);
                    continue;
                }

                RouteStruct.Response? staticFileResponse = staticFileExists(requestURL);
                if (staticFileResponse != null)
                {
                    await SendMethodes.handleResponse(resp, staticFileResponse);
                    continue;
                }

                await SendMethodes.handleResponse(resp, await getErrorResponse(parsedRequest));
                continue;
            }
        }

        public static async Task<RouteStruct.Response> getErrorResponse(RouteStruct.Request request)
        {
            if (StorageUtil.CustomError == null) return await Task.FromResult(StorageUtil.DefaultErrorResponse);

            RouteStruct.Response routeResponse = new RouteStruct.Response();
            await StorageUtil.CustomError.Value.Callback(request, routeResponse);

            return await Task.FromResult(routeResponse);
        }

        private static RouteStruct.Response? staticFileExists(string requestURL)
        {
            foreach (StaticFileStruct staticFile in StorageUtil.StaticFiles)
            {
                if (requestURL.StartsWith(staticFile.WebPath))
                {
                    string filePath = requestURL.Replace(staticFile.WebPath, "");
                    if (File.Exists(staticFile.DirectoryPath.FullName + "/" + filePath))
                    {
                        return new RouteStruct.Response
                        {
                            ResponseType = ResponseType.SENDFILE,
                            Data = staticFile.DirectoryPath.FullName + "/" + filePath
                        };
                    }
                }
            }

            return null;
        }
    }
}