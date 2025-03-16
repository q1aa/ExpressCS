# Express.CS 🚀

Welcome to the **Express.CS**! This project is a simple yet powerful server built with C# that demonstrates various functionalities such as routing, file handling, custom headers, middleware, and custom error handling.
The usage is designed to be as similar as <a href="https://github.com/expressjs/express">express.js</a> as possible, also this project inspired me :D

## Features ✨

- [**Routing**](#Routing): Handle different routes with ease.
- [**File Handling**](#file-download): Download and send files effortlessly.
- [**Custom Headers**](#custom-headers): Add and manage custom headers.
- [**Middleware**](#middleware): Implement middleware for additional processing.
- [**Custom Error Handling**](#custom-error-handling): Define custom error pages.
- [**Razor Engine Support**](#using-razor):  Integrate Razor for dynamic HTML generation.
- [**WebSocket Support**](#WebSockets): Easily enable real-time communication with WebSockets.

## Getting Started 🛠️

### Prerequisites

- .NET SDK (mine is 8.0.+)
- A code editor like Visual Studio Code, or Visual Studio Community Edition as I prefer.

### Installation

1. Create a new C# project:
    ```sh
    dotnet new console -n ExpressCS-Project
    ```

2. Navigate to the project directory:
    ```sh
    cd ExpressCS-Project
    ```

3. [download](https://github.com/q1aa/ExpressCS/releases/tag/Default) the dll file and add it to the project by just adding these three lines inside the .csproj file
    ```
    <ItemGroup>
        <Reference Include="ExpressCS.dll" />
    </ItemGroup>
    ```

4. Open the `Program.cs` file and add the necessary using directives to import your DLL:
    ```csharp
    using ExpressCS;
    ```

    4.1 For a simple "Hello World" example, check out [this link](#hello-world-example).

    4.2 For a Example how to use the Razor engine with it, [this link](#using-razor).

    4.3 For a Example how to use WebSockets, [this link](#WebSockets).
5. Run the project:
    ```sh
    dotnet run
    ```

## Configuration ⚙️
<p>Use a custom port (the default one is 8080)</p>

``` cs
ConfigStruct config = new ExpressCS.ExpressCS().CreateConfig(port: 3000);
//Change the startup to look like this:
server.StartUp(config);
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

### JSON Body
Handling JSON body, you may add a null check here too!
```csharp
server.RegisterRoute("/json", HttpMethod.POST, async (req, res) =>
{
    string? name = req.JSONBody["name"];
    string? age = req.JSONBody["age"];
    res.Send($"Welcome {name} with the age of {age}");
});
```

### Query Route

Handles query parameters and sends a response:
```csharp
server.RegisterRoute("/query", HttpMethod.GET, async (req, res) =>
{
    res.Send($"<html><body><h1>Query route {req.Url} with querys: {req.QueryParams["name"]} and {req.QueryParams["age"]}</h1></body></html>");
});
```

### Dynamic Route

Handles query parameters and sends a response:
```csharp
server.RegisterRoute("/dynamic/:id/:id2", HttpMethod.GET, async (req, res) =>
{
    NameValueCollection? reqParams = req.DynamicParams;
    res.Send($"Dynamic route with id: {reqParams["id"]} and id2: {reqParams["id2"]}");    
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

### Send JSON

Response with JSON
```csharp
server.RegisterRoute("/json", HttpMethod.GET, async (req, res) =>
{
    res.SendJSON(new { name = "John", age = 30 });
});
```

### Custom Headers

Adds custom headers to the response:
```csharp
server.RegisterRoute("/headers", HttpMethod.ANY, async (req, res) =>
{
    res.AddHeaders(new NameValueCollection
    {
        { "Content-Type", "text/html" },
        { "Custom-Header", "Custom-Header-Hello" }
    });
    res.Send($"<html><body><h1>Custom headers: {res.Headers.AllKeys.Select(key => 
    $"{key}: {res.Headers[key]}").Aggregate((a, b) => 
    $"{a}, {b}")}</h1></body></html>");
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

# WebSockets
### Register a WebSocket route
``` csharp
server.RegisterWebSocket("/ws", async (req, res) =>
{
    res.Data = $"Hello World!, message: {req.Data}";
});
```

### Register a WebSocket route with a dynamic URL
``` csharp
server.RegisterWebSocket("/dynamic/:id1/:id2", async (req, res) =>
{
    res.Data = $"Dynamic route with params: {req.DynamicParams.AllKeys.Select(key => $"{key}: {req.DynamicParams[key]}").Aggregate((a, b) => $"{a}, {b}")}";
});
```

### Register a WebSocket route with a Callback when the connection is established
``` csharp
server.RegisterWebSocket("/ws", async (req, res) =>
{
    res.Data = $"Hello World!, message: {req.Data}";
}, async (req, res) =>
{
    Console.WriteLine("Callback, Connection established");
    res.Data = "Connection established";
});
```

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

# Using Razor
<p name="using-razor"></p>
Here you can see a example how to use Razor

```csharp
server.RegisterRoute("/cshtml", HttpMethod.GET, async (req, res) =>
{
    var engine = new RazorLightEngineBuilder()
   .UseEmbeddedResourcesProject(typeof(Program))
   .SetOperatingAssembly(typeof(Program).Assembly)
   .UseMemoryCachingProvider()
   .Build();

    //Those are the replacing values 
    var replaceValues = new
    {
        Name = "World",
        Title = "Hello",
        ID = new Random().Next(int.MaxValue - 5389, int.MaxValue)
    };

    //you you read the content out of a file
    string fileContent = await File.ReadAllTextAsync(Directory.GetCurrentDirectory() + "/Views/test.cshtml");

    //Or just create it inline
    string inlineContent = "<html><body><h1>Hello @Model.Name, @Model.Title, @Model.ID</h1></body></html>"

    //Here the page will getting rendered
    string result = await engine.CompileRenderStringAsync("templateKey", fileContent, replaceValues);
    //Which we just send over to the client afterwards
    res.Send(result);
});
```

by the way, the test.cshtml file looks like this

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Welcome</title>
</head>
<body>
    <h1>Hello, @Model.Name!</h1>
    <p>Welcome to @Model.Title.</p>
    <p>
        provided id is @Model.ID
    </p>
</body>
</html>

```

## Errors/TODO 📓
"The specified network name is no longer available." -> inside the config set ignoreWriteExceptions: true ✅

"Access is denied." -> start the application as administrator and inside the config set
host: "*" ✅

TODO: Fix the memory leaking when accessing/uploading files ❌

## Contributing 🤝

Contributions are welcome! Please fork the repository and submit a pull request.

## License 📄

This project is licensed under the MIT License.
---
Made with ❤️ by [Julin](https://github.com/q1aa)
