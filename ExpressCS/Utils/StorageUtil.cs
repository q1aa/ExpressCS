using ExpressCS.Struct;
using ExpressCS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    internal class StorageUtil
    {
        //public static ConfigStruct? ServerConfig { get; set; } = null;
        public static HttpListener Listener { get; set; } = new HttpListener();
        public static List<RouteStruct> Routes { get; set; } = new List<RouteStruct>();
        public static List<WebSocketRouteStruct> WebSocketRoutes { get; set; } = new List<WebSocketRouteStruct>();
        public static RouteStruct? CustomError { get; set; } = null;
        public static RouteStruct? Middleware { get; set; } = null;
        public static List<StaticFileStruct> StaticFiles { get; set; } = new List<StaticFileStruct>();
        public static bool CalculateDataSize { get; set; } = true;

        public static readonly RouteStruct.Response DefaultErrorResponse = new RouteStruct.Response
        {
            ContentType = "text/html",
            ContentEncoding = Encoding.UTF8,
            StatusCode = 404,
            Data = "<html><body><h1>404 Not Found</h1></body></html>",
            ResponseType = ResponseType.DATA
        };
    }

    class TransferSizeStorage
    {
        public static long TotalDownloadSize { get; set; } = 0;
        public static long TotalUploadSize { get; set; } = 0;
    }
}
