using ExpressCS.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS
{
    internal class SendMethodes
    {
        public static async Task<bool> handleResponse(HttpListenerResponse resp, RouteStruct.Response routeResponse)
        {
            switch (routeResponse.ResponseType)
            {
                case ResponseType.DATA:
                    return await sendResponse(resp, routeResponse);
                case ResponseType.DOWNLOAD:
                    return await downloadFile(resp, routeResponse.Data, routeResponse.FileName);
                case ResponseType.SENDFILE:
                    return await sendFile(resp, routeResponse.Data);
                case ResponseType.REDIRECT:
                    return await redirect(resp, routeResponse.Data);
                default:
                    return false;
            }
        }
        public static async Task<bool> sendResponse(HttpListenerResponse resp, RouteStruct.Response routeResponse)
        {
            byte[] data = Encoding.UTF8.GetBytes(routeResponse.Data);
            resp.ContentType = routeResponse.ContentType ?? "text/html";
            resp.ContentEncoding = routeResponse.ContentEncoding ?? Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;
            resp.StatusCode = routeResponse.StatusCode;

            await resp.OutputStream.WriteAsync(data, 0, data.Length);

            resp.Close();

            return true;
        }

        public static async Task<bool> downloadFile(HttpListenerResponse resp, string filePath, string fileName = null)
        {
            byte[] data = File.ReadAllBytes(filePath);
            resp.ContentType = "application/octet-stream";
            resp.ContentLength64 = data.LongLength;
            resp.StatusCode = 200;

            if (fileName != null)
            {
                resp.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
            }
            else
            {
                resp.AddHeader("Content-Disposition", $"attachment; filename={Path.GetFileName(filePath)}");
            }

            await resp.OutputStream.WriteAsync(data, 0, data.Length);

            resp.Close();

            return true;
        }

        public static async Task<bool> sendFile(HttpListenerResponse resp, string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            resp.ContentType = "text/html";
            resp.ContentLength64 = data.LongLength;
            resp.StatusCode = 200;

            await resp.OutputStream.WriteAsync(data, 0, data.Length);

            resp.Close();

            return true;
        }

        public static async Task<bool> redirect(HttpListenerResponse resp, string path)
        {
            resp.StatusCode = 302;
            resp.RedirectLocation = path;
            resp.Close();

            return true;
        }
    }
}
