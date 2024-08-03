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

namespace ExpressCS
{
    internal class Server
    {
        public static async Task HandleIncomeRequests()
        {
            while (true)
            {
                HttpListenerContext ctx = await StorageUtil.Listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if (req.HttpMethod == "HEAD")
                {
                    resp.Close();
                    continue;
                }

                if(StorageUtil.Middleware != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await StorageUtil.Middleware.Value.Callback(new RouteStruct.Request
                    {
                        Url = req.Url.AbsolutePath,
                        Method = req.HttpMethod,
                        Host = req.UserHostName,
                        UserAgent = req.UserAgent
                    }, routeResponse);

                    if(routeResponse.Data != null)
                    {
                        await sendResponse(resp, routeResponse);
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

                if(foundRoute != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await foundRoute.Value.Callback(new RouteStruct.Request
                    {
                        Url = req.Url.AbsolutePath,
                        Method = req.HttpMethod,
                        Host = req.UserHostName,
                        UserAgent = req.UserAgent,
                        Body = req.HasEntityBody ? new StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd() : null,
                        DynamicParams = getDynamicParamsFromURL(foundRoute.Value, req.Url.AbsolutePath),
                        QueryParams = getQueryParamsFromURL(req.RawUrl)
                    }, routeResponse);

                    await sendResponse(resp, routeResponse);
                    continue;
                }


                if (StorageUtil.CustomError != null)
                {
                    RouteStruct.Response routeResponse = new RouteStruct.Response();
                    await StorageUtil.CustomError.Value.Callback(new RouteStruct.Request
                    {
                        Url = req.Url.AbsolutePath,
                        Method = req.HttpMethod,
                        Host = req.UserHostName,
                        UserAgent = req.UserAgent,
                    }, routeResponse);

                    await sendResponse(resp, routeResponse);
                    continue;
                }
                await sendResponse(resp, new RouteStruct.Response
                {
                    Data = "<html><body><h1>404 Not Found</h1></body></html>",
                    ContentType = "text/html",
                    ContentEncoding = Encoding.UTF8,
                    ContentLength64 = Encoding.UTF8.GetByteCount("<html><body><h1>404 Not Found</h1></body></html>")
                });
            }
        }

        private static async Task<bool> sendResponse(HttpListenerResponse resp, RouteStruct.Response routeResponse)
        {
            byte[] data = Encoding.UTF8.GetBytes(routeResponse.Data);
            resp.ContentType = routeResponse.ContentType ?? "text/html";
            resp.ContentEncoding = routeResponse.ContentEncoding ?? Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;

            await resp.OutputStream.WriteAsync(data, 0, data.Length);

            resp.Close();

            return true;
        }

        private static string[] getDynamicParamsFromURL(RouteStruct route, string url)
        {
            string[] routePath = route.Path.Split('/');
            string[] reqPath = url.Split('/');

            List<string> dynamicParams = new List<string>();

            for (int i = 0; i < routePath.Length; i++)
            {
                if (routePath[i].StartsWith(":"))
                {
                    dynamicParams.Add(reqPath[i]);
                }
            }

            if(dynamicParams.Count == 0) return null;
            return dynamicParams.ToArray();
        }

        private static string[] getQueryParamsFromURL(string url)
        {
            Console.WriteLine("url" + url);
            if (!url.Contains("?")) return null;

            Console.WriteLine(url.Split('?')[1].Split('&'));
            return url.Split('?')[1].Split('&');
        }
    }
}
