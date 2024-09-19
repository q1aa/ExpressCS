using System.Net;
using System.Runtime.CompilerServices;
using ExpressCS.Struct;
using ExpressCS.Types;
using ExpressCS.Utils;

namespace ExpressCS
{
    public class ExpressCS
    {
        public ConfigStruct CreateConfig(int port = 8080, string host = "localhost", bool ssl = false, bool showTransferedDataSize = true)
        {
            return new ConfigStruct(port, host, ssl, showTransferedDataSize);
        }

        public Task<bool> StartUp(ConfigStruct config)
        {
            StartUp(config, () => Task.CompletedTask);
            return Task.FromResult(true);
        }

        public Task<bool> StartUp(ConfigStruct config, Func<Task> callback)
        {
            if(NetworkHelper.IsPortInUse(config.Port))
            {
                LogUtil.LogError($"Application shutting down! Port {config.Port} is already in use, please use another port or close the application using the port");
                return Task.FromResult(false);
            }

            StorageUtil.Listener = new HttpListener();
            StorageUtil.Listener.Prefixes.Add($"{(config.Ssl ? "https" : "http")}://{config.Host}:{config.Port}/");
            StorageUtil.Listener.Start();
            
            LogUtil.Log($"Server started on {config.Host}:{config.Port}");
            LogUtil.Log($"Access the server at {StorageUtil.Listener.Prefixes.FirstOrDefault()})");

            LogUtil.Log("---------Registered Static Files---------", prefix: false);
            foreach (var staticFile in StorageUtil.StaticFiles)
            {
                if (!staticFile.DirectoryPath.Exists)
                {
                    LogUtil.LogWarning($"Directory {staticFile.DirectoryPath.FullName} does not exist");
                    continue;
                }
                LogUtil.LogPublicDirectory(staticFile.WebPath, staticFile.DirectoryPath.FullName, staticFile.DirectoryPath.GetFiles().Length);
            }

            if (StorageUtil.Routes.Count > 0) LogUtil.Log("---------Registered Routes---------", prefix: false);
            foreach (var routes in StorageUtil.Routes)
            {
                LogUtil.LogRouteRegister(routes.Path, routes.Methods);
            }

            if (StorageUtil.WebSocketRoutes.Count > 0)
                LogUtil.Log("---------Registered WebSocket Routes---------", prefix: false);
            foreach (var route in StorageUtil.WebSocketRoutes)
            {
                LogUtil.LogRouteRegister(route.Path, new Struct.HttpMethod[] { }, true);
            }
            if(StorageUtil.WebSocketRoutes.Count > 0) Console.WriteLine("---------------------------------");

            Task listenTask = Server.HandleIncomeRequests(config.ShowTransferedDataSize);
            if(config.ShowTransferedDataSize) SizeUpdaterUtil.StartTransferUpdateTimer();

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
            LogUtil.Log("Custom error page registered" + (StorageUtil.CustomError != null ? " successfully" : " unsuccessfully"));
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

        public Task<bool> RegisterWebSocket(string path, Func<WebSocketRouteStruct.WebSocketRequest, WebSocketRouteStruct.WebSocketResponse, Task> callback, int messageBytes = 4096)
        {
            return RegisterWebSocket(path, callback, null, messageBytes);
        }
    }
}
