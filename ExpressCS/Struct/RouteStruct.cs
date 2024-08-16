using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public RouteStruct(string path, HttpMethod[] method, Func<Request, Response, Task> callback, string[] dynamicContent)
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
            public string Body { get; set; }
            public NameValueCollection JSONBody { get; set; }
            public NameValueCollection Headers { get; set; }
            public string ContentType { get; set; }
            public string[]? DynamicParams { get; set; }
            public Dictionary<string, string>? QueryParams { get; set; }
        }

        public class Response
        {
            public string ContentType { get; set; }
            public Encoding ContentEncoding { get; set; }
            public long ContentLength64 { get; set; }
            public string Data { get; set; } = null;
            public int StatusCode { get; set; } = -1;
            public List<string> Headers { get; set; }
            public ResponseType ResponseType { get; set; }

            //only for the download function, to set a custom filename
            public string FileName { get; set; }

            public void Send(string data)
            {
                Data = data;
                ResponseType = ResponseType.DATA;
            }

            public void Download(string filePath, string fileName = null)
            {
                Data = filePath;
                ResponseType = ResponseType.DOWNLOAD;
                FileName = fileName;
            }

            public void SendFile(string filePath)
            {
                Data = filePath;
                ResponseType = ResponseType.SENDFILE;
            }

            public void Redirect(string path)
            {
                Data = path;
                ResponseType = ResponseType.REDIRECT;
            }
        }
    }
}
