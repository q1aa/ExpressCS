using ExpressCS.Struct;
using ExpressCS.Types;
using ExpressCS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

                RouteStruct? foundRoute = null;

                foreach (RouteStruct route in StorageUtil.Routes)
                {
                    if (route.Path == req.Url.AbsolutePath && route.Method.ToString() == req.HttpMethod)
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
                        UserAgent = req.UserAgent
                    }, routeResponse);

                    await sendResponse(resp, routeResponse);
                }

                else
                {
                    await sendResponse(resp, new RouteStruct.Response
                    {
                        Data = "<html><body><h1>404 Not Found</h1></body></html>",
                        ContentType = "text/html",
                        ContentEncoding = Encoding.UTF8,
                        ContentLength64 = Encoding.UTF8.GetByteCount("<html><body><h1>404 Not Found</h1></body></html>")
                    });
                }
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
    }
}
