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
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Buffers;


namespace ExpressCS
{
    internal class Server
    {
        public static async Task HandleIncomeRequests(bool showTransferedDataSize, int maxRequestSizeInBytes)
        {
            while (true)
            {
                HttpListenerContext ctx = await StorageUtil.Listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                ReceiveFileStruct[]? files = null;

                if (maxRequestSizeInBytes > 0 && req.ContentLength64 > maxRequestSizeInBytes)
                {
                    resp.StatusCode = 413;
                    resp.Close();
                    continue;
                }

                string? rawBody = null;
                string? boundary = null;
                bool fileUpload = false;

                try
                {
                    if (req.Url == null)
                    {
                        resp.Close();
                        continue;
                    }

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
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await WebSocketHandler.HandleSocketIitialization(ctx, foundWebSocketRoute.Value);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"WebSocket error: {ex.Message}");
                                }
                            });
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

                    //TODO: Add support for multipart/form-data
                    if (req.ContentType != null && req.ContentType.Contains("multipart/form-data") && req.ContentType.Contains("boundary="))
                    {
                        using (Stream bodyStream = HelperUtil.CopyInputStream(req.InputStream))
                        {
                            boundary = req.ContentType.Split("boundary=")[1];
                            rawBody = new StreamReader(bodyStream, req.ContentEncoding).ReadToEnd();
                            bodyStream.Position = 0;
                            files = await UploadFileUtil.HandleFileUpload(req, resp, bodyStream);
                            Console.WriteLine("File upload detected");
                        }
                        fileUpload = true;
                    }
                    else
                    {
                        /*StringBuilder sb = new StringBuilder();
                        char[] buffer = ArrayPool<char>.Shared.Rent(4096);

                        try
                        {
                            using (StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
                            {
                                int charsRead;
                                while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    sb.Append(buffer, 0, charsRead);
                                }
                            }
                        }
                        finally
                        {
                            ArrayPool<char>.Shared.Return(buffer);
                        }

                        rawBody = sb.ToString();
                        sb.Clear();*/

                        using (Stream bodyStream = HelperUtil.CopyInputStream(req.InputStream))
                        {
                            rawBody = new StreamReader(bodyStream, req.ContentEncoding).ReadToEnd();
                            bodyStream.Position = 0;
                        }
                    }

                    RouteStruct.Request parsedRequest = new RouteStruct.Request()
                    {
                        Url = req.Url.AbsolutePath,
                        Method = req.HttpMethod,
                        Host = req.UserHostName,
                        UserAgent = req.UserAgent,
                        Body = rawBody ?? "",
                        JSONBody = new NameValueCollection(), //fileUpload ? new NameValueCollection() : HelperUtil.parseJSONBody(rawBody),
                        FormDataBody = HelperUtil.parseFormDataBody(rawBody, boundary),
                        QueryParams = HelperUtil.getQueryParamsFromURL(req.Url.PathAndQuery),
                        ContentType = req.ContentType,
                        Headers = req.Headers,
                        DynamicParams = null,
                        Files = files
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
                            RouteStruct.Response errorRouteResponse = new RouteStruct.Response();
                            await StorageUtil.Middleware.Value.Callback(parsedRequest, errorRouteResponse);
                            if (errorRouteResponse.Data != null)
                            {
                                await SendMethodes.handleResponse(resp, errorRouteResponse);
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


                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    if (StorageUtil.Middleware != null)
                    {
                        await StorageUtil.Middleware.Value.Callback(parsedRequest, routeResponse);
                        if (routeResponse.Data != null)
                        {
                            await SendMethodes.handleResponse(resp, routeResponse);
                            continue;
                        }
                    }

                    if (foundRoute != null)
                    {
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    resp.StatusCode = 500;
                    resp.Close();
                    continue;
                }
                finally
                {
                    foreach (ReceiveFileStruct file in files ?? new ReceiveFileStruct[0])
                    {
                        file.Dispose();
                    }

                    rawBody = null;
                    boundary = null;

                    ctx.Response.Close();
                    ctx.Request.InputStream.Close();
                    Marshal.FreeHGlobal(Marshal.AllocHGlobal(1));
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
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