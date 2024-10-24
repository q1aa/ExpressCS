using ExpressCS.Struct;
using ExpressCS.Utils;
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
            if (resp == null) return false;

            routeResponse.Headers?.ForEach(header =>
            {
                resp.AddHeader(header.Split(':')[0], header.Split(':')[1]);
            });

            if (routeResponse.Data == null) return false;
            UploadSizeUtil.AddUploadSize(routeResponse.Headers, routeResponse.Data.Length);

            switch (routeResponse.ResponseType)
            {
                case ResponseType.DATA:
                    return await sendResponse(resp, routeResponse);
                case ResponseType.DOWNLOAD:
                    return await downloadFile(resp, routeResponse);
                case ResponseType.SENDFILE:
                    return await sendFile(resp, routeResponse);
                case ResponseType.REDIRECT:
                    return await redirect(resp, routeResponse);
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
            resp.StatusCode = HelperUtil.getStatusCode(routeResponse.StatusCode, 200);

            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
            return true;
        }

        public static async Task<bool> downloadFile(HttpListenerResponse resp, RouteStruct.Response routeResponse)
        {
            if(!File.Exists(routeResponse.FileName))
            {
                LogUtil.LogError($"File {routeResponse.FileName} not found");
                resp.StatusCode = 404;
                resp.Close();
                return false;
            }

            byte[] data = File.ReadAllBytes(routeResponse.FileName);
            resp.ContentType = "application/octet-stream";
            resp.ContentLength64 = data.LongLength;
            resp.StatusCode = HelperUtil.getStatusCode(routeResponse.StatusCode, 200);

            if (routeResponse.FileName != null)
            {
                resp.AddHeader("Content-Disposition", $"attachment; filename={routeResponse.FileName}");
            }
            else
            {
                resp.AddHeader("Content-Disposition", $"attachment; filename={Path.GetFileName(routeResponse.Data)}");
            }

            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
            return true;
        }

        public static async Task<bool> sendFile(HttpListenerResponse resp, RouteStruct.Response routeResponse)
        {
            if(!File.Exists(routeResponse.Data))
            {
                LogUtil.LogError($"File {routeResponse.Data} not found");
                resp.StatusCode = 404;
                resp.Close();
                return false;
            }

            using (FileStream fs = new FileStream(routeResponse.Data, FileMode.Open, FileAccess.Read))
            {
                resp.ContentType = HelperUtil.getContentType(Path.GetExtension(routeResponse.Data));
                resp.ContentLength64 = fs.Length;
                resp.StatusCode = HelperUtil.getStatusCode(routeResponse.StatusCode, 200);

                byte[] buffer = new byte[64 * 1024];
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await resp.OutputStream.WriteAsync(buffer, 0, read);
                }
            }
            resp.Close();
            return true;
        }

        public static async Task<bool> redirect(HttpListenerResponse resp, RouteStruct.Response routeResponse)
        {
            resp.RedirectLocation = routeResponse.Data;
            resp.StatusCode = HelperUtil.getStatusCode(routeResponse.StatusCode, 302);
            resp.Close();
            return true;
        }
    }
}
