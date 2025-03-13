using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            public string Url { get; internal set; }
            public string Method { get; internal set; }
            public string Host { get; internal set; }
            public string UserAgent { get; internal set; }
            public string Body { get; internal set; }
            public NameValueCollection? JSONBody { get; internal set; }
            public NameValueCollection? FormDataBody { get; internal set; }
            public NameValueCollection Headers { get; internal set; }
            public string ContentType { get; internal set; }
            public NameValueCollection? DynamicParams { get; internal set; }
            public NameValueCollection? QueryParams { get; internal set; }
            public ReceiveFileStruct[]? Files { get; internal set; }
        }

        public class Response
        {
            public string ContentType { get; set; }
            public Encoding ContentEncoding { get; set; }
            public long ContentLength64 { get; set; }
            public string Data { get; set; } = null;
            public int StatusCode { get; set; } = -1;
            public NameValueCollection Headers { get; } = new NameValueCollection();
            public ResponseType ResponseType { get; set; }

            //only for the download function, to set a custom filename
            public string? FileName { get; private set; }

            public void Send(string data)
            {
                Data = data;
                ResponseType = ResponseType.DATA;
            }

            public void SendJSON<T>(T data)
            {
                Data = JsonSerializer.Serialize(data);
                ResponseType = ResponseType.DATA;
                ContentType = "application/json";
            }

            public void Download(string filePath, string? fileName = null)
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

            public void AddHeaders(NameValueCollection headers)
            {
                headers.AllKeys.ToList().ForEach(key => Headers.Add(key, headers[key]));
            }

            public void AddHeader(string key, string value)
            {
                Headers.Add(key, value);
            }
        }
    }
}
