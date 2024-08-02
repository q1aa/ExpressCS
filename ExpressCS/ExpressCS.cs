using ExpressCS.Struct;
using ExpressCS.Types;
using ExpressCS.Utils;
using System.Net;

namespace ExpressCS
{
    public class ExpressCS
    {
        public ConfigStruct CreateConfig(int port = 8080, string host = "localhost", bool ssl = false)
        {
            return new ConfigStruct(port, host, ssl);
        }

        public Task<bool> StartUp(ConfigStruct config)
        {
            StorageUtil.Listener = new HttpListener();
            StorageUtil.Listener.Prefixes.Add($"{(config.Ssl ? "https" : "http")}://{config.Host}:{config.Port}/");
            StorageUtil.Listener.Start();
            Console.WriteLine($"Server started on {config.Host}:{config.Port}");

            Task listenTask = Server.HandleIncomeRequests();
            listenTask.GetAwaiter().GetResult();
            StorageUtil.Listener.Close();

            return Task.FromResult(true);
        }

        public Task<bool> RegisterRoute(string path, HttpMethod method, Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            StorageUtil.Routes.Add(new RouteStruct
            {
                Path = path,
                Method = method,
                Callback = callback
            });

            Console.WriteLine($"Registered route: {path}");


            return Task.FromResult(true);
        }

        public Task<bool> CustomError(Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            return Task.FromResult(true);
        }
    }
}
