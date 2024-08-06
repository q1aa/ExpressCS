# Express.CS 🚀

Welcome to the **Express.CS**! This project is a simple yet powerful server built with C# that demonstrates various functionalities such as routing, file handling, custom headers, middleware, and custom error handling.
The usage is designed to be as similar as <a href="https://github.com/expressjs/express">express.js</a> as possible, also tis project inspired me :D

## Features ✨

- **Routing**: Handle different routes with ease.
- **File Handling**: Download and send files effortlessly.
- **Custom Headers**: Add and manage custom headers.
- **Middleware**: Implement middleware for additional processing.
- **Custom Error Handling**: Define custom error pages.

## Getting Started 🛠️

### Prerequisites

- .NET SDK
- A code editor like Visual Studio Code, or Visual Studio Community Edition as i prefer.

### Installation

1. Download the DLL from the repository:
    ```sh
    curl -L -o ExpressCS.dll https://github.com/q1aa/ExpressCS/releases/latest/download/ExpressCS.dll
    ```

2. Create a new C# project:
    ```sh
    dotnet new console -n MyNewProject
    ```

3. Navigate to the project directory:
    ```sh
    cd MyNewProject
    ```

4. Add the downloaded DLL to the project:
    ```sh
    dotnet add reference ../ExpressCS.dll
    ```

5. Restore the dependencies:
    ```sh
    dotnet restore
    ```

6. Open the `Program.cs` file and add the necessary using directives to import your DLL:
    ```csharp
    using ExpressCS;
    ```

    6.1 For a simple "Hello World" example, refer to [this link](#hello-world-example).

7. Build and run the project:
    ```sh
    dotnet run
    ```

## Configuration ⚙️
<p>Use a custom port (the default one is 8080)</p>

``` cs
ConfigStruct config = new ExpressCS.ExpressCS().CreateConfig(port: 3000);
```

<p>Callback after server started up</p>

``` cs
server.StartUp(config, async () =>
    {
        //Do something after the startup, for example print a message
        ExpressCS.Utils.LogUtil.LogInfo($"Server started up on {(config.Ssl ? "https" : "http")}://{config.Host}:{config.Port}");
    }).GetAwaiter().GetResult();
```

<p>Static folder for accessing files</p>

``` cs
//first argument is the path, the second one is the folder, in my case the folder is named "static" and is in the same directory as my executable
server.StaticDirectory("/static", new DirectoryInfo("static"));
```

## Routing

A route always gets registered as following:

``` cs
//First argument is the path,
//The second argument is the HTTP Method,
//The third argument is the async callback function

using HttpMethod = ExpressCS.Struct.HttpMethod;
server.RegisterRoute("/hello", HttpMethod.GET, async (req, res) => { 
    //Code after route gets called
});
```

You might want to use the same route on a different HTTP Method too?
``` cs
server.RegisterRoute("/test", new HttpMethod[] { HttpMethod.GET, HttpMethod.POST }, async (req, res) => {
    //Code after route gets called
});
```
Or just for all HTTP Methods use this code:
``` cs
server.RegisterRoute("/test", HttpMethod.ANY, async (req, res) => {
    //Code after route gets called
});
```

Here are some examples of the routing you might want to use:

### Query Route

Handles query parameters and sends a response:
```csharp
server.RegisterRoute("/query", HttpMethod.GET, async (req, res) =>
{
    res.Send($"<html><body><h1>Query route {req.Url} with querys: {queryParams["name"]} and {queryParams["age"]}</h1></body></html>");
});
```

### File Download

Allows downloading a file:
```csharp
server.RegisterRoute("/download", HttpMethod.GET, async (req, res) =>
{
    res.Download("static/test.txt", "filename_test.txt");
});
```

### Send File

Sends a file to the client:
```csharp
server.RegisterRoute("/sendfile", HttpMethod.GET, async (req, res) =>
{
    res.SendFile("static/test.pdf");
});
```

### Redirect

Redirects to another route:
```csharp
server.RegisterRoute("/redirect", HttpMethod.GET, async (req, res) =>
{
    res.Redirect("/test");
});
```

### Custom Headers

Adds custom headers to the response:
```csharp
server.RegisterRoute("/headers", HttpMethod.ANY, async (req, res) =>
{
    res.Headers = new List<string> { "Content-Type: text/html", "Custom-Header: test" };
    res.Send($"<html><body><h1>Custom headers {res.Headers[0]} and {res.Headers[1]}</h1></body></html>");
});
```

### Custom Error Handling

Defines a custom 404 error page:
```csharp
server.CustomError(async (req, res) =>
{
    res.StatusCode = 404;
    res.Send("<html><body><h1>404 Not Found (custom)</h1></body></html>");
});
```

### Middleware

Logs the request URL:
```csharp
server.MiddleWare(async (req, res) =>
{
    Console.WriteLine($"Middleware: {req.Url}");
});
```

## Contributing 🤝

Contributions are welcome! Please fork the repository and submit a pull request.

## License 📄

This project is licensed under the MIT License.

# Hello World Example
<p name="hello-world-example"></p>

``` cs
using ExpressCS.Types;
using HttpMethod = ExpressCS.Struct.HttpMethod;

public static class Program
{
    static ExpressCS.ExpressCS server = new ExpressCS.ExpressCS();
    public static void Main()
    {
        ConfigStruct config = new ExpressCS.ExpressCS().CreateConfig();

        server.RegisterRoute("/hello", HttpMethod.GET, async (req, res) =>
        {
            res.StatusCode = 200;
            res.Send("world");
        });

        server.StartUp(config);
    }
}
```

---

Made with ❤️ by [Julin](https://github.com/q1aa)