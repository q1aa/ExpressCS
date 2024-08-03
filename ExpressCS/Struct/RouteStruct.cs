using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Struct
{
    public struct RouteStruct
    {
        public string Path { get; set; }
        public HttpMethod[] Methods { get; set; }

        public Func<Request, Response, Task> Callback { get; set; }

        public RouteStruct(string path, HttpMethod[] method, Func<Request, Response, Task> callback)
        {
            Path = path;
            Methods = method;
            Callback = callback;
        }

        public class Request
        {
            public string Url { get; set; }
            public string Method { get; set; }
            public string Host { get; set; }
            public string UserAgent { get; set; }
        }

        public class Response
        {
            public string ContentType { get; set; }
            public Encoding ContentEncoding { get; set; }
            public long ContentLength64 { get; set; }
            public string Data { get; set; }
        }
    }
}
