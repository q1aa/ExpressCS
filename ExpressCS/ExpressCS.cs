using System.Net;
using ExpressCS.Struct;
using ExpressCS.Types;
using ExpressCS.Utils;

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
            StartUp(config, () => Task.CompletedTask);
            return Task.FromResult(true);
        }

        public Task<bool> StartUp(ConfigStruct config, Func<Task> callback)
        {
            StorageUtil.Listener = new HttpListener();
            StorageUtil.Listener.Prefixes.Add($"{(config.Ssl ? "https" : "http")}://{config.Host}:{config.Port}/");
            StorageUtil.Listener.Start();

            Console.WriteLine("---------------------------------");
            foreach (var routes in StorageUtil.Routes)
            {
                LogUtil.LogRouteRegister(routes.Path, routes.Methods);
            }
            Console.WriteLine("---------------------------------");
            foreach (var route in StorageUtil.WebSocketRoutes)
            {
                LogUtil.LogRouteRegister(route.Path, new Struct.HttpMethod[] { }, true);
            }
            Console.WriteLine("---------------------------------");

            Task listenTask = Server.HandleIncomeRequests();
            callback.Invoke();
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

            return Task.FromResult(true);
        }

        public Task<bool> RegisterRoute(string path, Struct.HttpMethod method, Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            return RegisterRoute(path, new Struct.HttpMethod[] { method }, callback);
        }

        public Task<bool> RegisterRoute(string path, Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            return RegisterRoute(path, new Struct.HttpMethod[] { Struct.HttpMethod.ANY }, callback);
        }

        public Task<bool> CustomError(Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            StorageUtil.CustomError = new Struct.RouteStruct
            {
                Callback = callback
            };
            LogUtil.Log("Custom error handler registered" + (StorageUtil.CustomError != null ? " successfully" : " unsuccessfully"));
            return Task.FromResult(true);
        }

        public Task<bool> MiddleWare(Func<RouteStruct.Request, RouteStruct.Response, Task> callback)
        {
            StorageUtil.Middleware = new Struct.RouteStruct
            {
                Callback = callback
            };
            LogUtil.Log("Middleware registered" + (StorageUtil.Middleware != null ? " successfully" : " unsuccessfully"));
            return Task.FromResult(true);
        }

        public Task<bool> StaticDirectory(string webPath, DirectoryInfo directory)
        {
            StorageUtil.StaticFiles.Add(new StaticFileStruct(webPath, directory));
            LogUtil.Log($"Registered static files: {webPath} -> {directory.FullName}");
            return Task.FromResult(true);
        }

        public Task<bool> RegisterWebSocket(string path, Func<WebSocketRouteStruct.WebSocketRequest, WebSocketRouteStruct.WebSocketResponse, Task> callback, Func<WebSocketRouteStruct.WebSocketRequest, WebSocketRouteStruct.WebSocketResponse, Task> connectionEstablished, int messageBytes = 4096)
        {
            StorageUtil.WebSocketRoutes.Add(new WebSocketRouteStruct
            {
                Path = path,
                Callback = callback,
                ConnectionEstablished = connectionEstablished,
                MessageBytes = messageBytes
            });

            return Task.FromResult(true);
        }
    }
}
