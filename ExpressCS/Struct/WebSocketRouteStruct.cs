using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Struct
{
    public struct WebSocketRouteStruct
    {
        public string Path { get; set; }
        public Func<WebSocketRequest, WebSocketResponse, Task> Callback { get; set; }
        public Func<WebSocketRequest, WebSocketResponse, Task> ConnectionEstablished { get; set; }
        public int MessageBytes { get; set; }

        public WebSocketRouteStruct(string path, Func<WebSocketRequest, WebSocketResponse, Task> callback, Func<WebSocketRequest, WebSocketResponse, Task> connectionEstablished, int messageBytes)
        {
            Path = path;
            Callback = callback;
            ConnectionEstablished = connectionEstablished;
            MessageBytes = messageBytes;
        }

        public class WebSocketRequest
        {
            public string Data { get; set; }
            public string Url { get; set; }
            public string Host { get; set; }
            public NameValueCollection Headers { get; set; }
            public NameValueCollection? DynamicParams { get; set; }
            public NameValueCollection? QueryParams { get; set; }
        }

        public class WebSocketResponse
        {
            public string Data { get; set; } = null;
            public List<string> Headers { get; set; }
        }
    }
}
