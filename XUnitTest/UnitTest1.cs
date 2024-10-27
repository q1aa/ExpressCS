using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using HttpMethod = ExpressCS.Struct.HttpMethod;

public class UnitTest1
{
    private readonly HttpClient _httpClient;

    public UnitTest1()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5002"), Timeout = TimeSpan.FromSeconds(5) };
    }

    private async Task StartServerAsync()
    {
        var serverTask = Task.Run(() => Program.Main());
        var isRunning = await WaitForServerToStartAsync();
        Assert.True(isRunning, "Server did not start in time.");
    }

    private async Task<bool> WaitForServerToStartAsync(int maxAttempts = 100, int delayBetweenAttempts = 1000)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync("/test");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {
                
            }

            await Task.Delay(delayBetweenAttempts);
        }

        return false;
    }

    [Theory()]
    [InlineData(HttpMethod.GET, "/test", 200, "<html><body><h1>Hello, world!</h1></body></html>")]
    [InlineData(HttpMethod.GET, "/calc", 200, null)]
    [InlineData(HttpMethod.GET, "/any", 200, "<html><body><h1>Any method GET for /any</h1></body></html>")]
    [InlineData(HttpMethod.GET, "/dynamic/1/2", 200, "Dynamic route with id: 1 and id2: 2")]
    [InlineData(HttpMethod.GET, "/query?name=John&query=Hello", 200, null)]
    // /download
    // /sendfile
    [InlineData(HttpMethod.GET, "/redirect", 200, null)]
    [InlineData(HttpMethod.GET, "/headers", 200, null)]
    // /json
    // /razor
    // /cshtml
    // /Upper
    // /prime
    [InlineData(HttpMethod.POST, "/any", 200, "<html><body><h1>Any method POST for /any</h1></body></html>")]
    [InlineData(HttpMethod.DELETE, "/any", 200, "<html><body><h1>Any method DELETE for /any</h1></body></html>")]
    [InlineData(HttpMethod.POST, "/post", 200, null)]
    [InlineData(HttpMethod.POST, "/calc", 200, null)]

    public async Task TestMultipleHttpRequest(HttpMethod httpMethod, string url, int expectedStatusCode, string expectedContent)
    {
        await StartServerAsync();

        HttpResponseMessage? response = null;
        switch (httpMethod)
        {
            case HttpMethod.GET:
                response = await _httpClient.GetAsync(url);
                break;
            case HttpMethod.POST:
                response = await _httpClient.PostAsync(url, new StringContent("test", Encoding.UTF8, "text/plain"));
                break;
            case HttpMethod.DELETE:
                response = await _httpClient.PutAsync(url, new StringContent("test", Encoding.UTF8, "text/plain"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(httpMethod), httpMethod, null);
        }

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
        if (expectedContent != null)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedContent, content);
        }
    }


    [Theory()]
    [InlineData("/ws", "Hello", "World")]
    [InlineData("/ws", "OtherMessage", "Unknown message: OtherMessage")]
    [InlineData("/dynamic/1/2", "Hello", "Dynamic route with id: 1 and id2: 2")]
    public async Task TestMultipleWebSockets(string path, string message, string expectedMessage)
    {
        await StartServerAsync();

        var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://localhost:5002{path}"), default);

        var buffer = new ArraySegment<byte>(new byte[1024]);
        await client.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, default);

        var result = await client.ReceiveAsync(buffer, default);
        var receivedMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

        Assert.Equal(WebSocketMessageType.Text, result.MessageType);
        //Assert.Equal(expectedMessage, receivedMessage);
    }
}
