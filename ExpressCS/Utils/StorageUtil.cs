using ExpressCS.Struct;
using ExpressCS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    internal class StorageUtil
    {
        //public static ConfigStruct? ServerConfig { get; set; } = null;
        public static HttpListener Listener { get; set; } = new HttpListener();
        public static List<RouteStruct> Routes { get; set; } = new List<RouteStruct>();
        public static RouteStruct? CustomError { get; set; } = null;
        public static RouteStruct? Middleware { get; set; } = null;
        public static List<StaticFileStruct> StaticFiles { get; set; } = new List<StaticFileStruct>();
    }
}
