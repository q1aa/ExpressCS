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

        public Task<bool> RegisterRoute(string path, Struct.HttpMethod[] method, Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            StorageUtil.Routes.Add(new RouteStruct
            {
                Path = path,
                Methods = method,
                Callback = callback
            });

            Console.WriteLine($"Registered route: {path}");

            return Task.FromResult(true);
        }

        public Task<bool> RegisterRoute(string path, Struct.HttpMethod method, Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            return RegisterRoute(path, new Struct.HttpMethod[] { method }, callback);
        }

        public Task<bool> CustomError(Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            StorageUtil.CustomError = new Struct.RouteStruct
            {
                Callback = callback
            };
            Console.WriteLine("Custom error handler registered" + (StorageUtil.CustomError != null ? " successfully" : " unsuccessfully"));
            return Task.FromResult(true);
        }

        public Task<bool> MiddleWare(Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            StorageUtil.Middleware = new Struct.RouteStruct
            {
                Callback = callback
            };
            Console.WriteLine("Middleware registered" + (StorageUtil.Middleware != null ? " successfully" : " unsuccessfully"));
            return Task.FromResult(true);
        }

        public Task<bool> StaticDirectory(string webPath, DirectoryInfo directory)
        {
            StorageUtil.StaticFiles.Add(new StaticFileStruct(webPath, directory));
            Console.WriteLine($"Registered static files: {webPath} -> {directory.FullName}");
            return Task.FromResult(true);
        }
    }
}
