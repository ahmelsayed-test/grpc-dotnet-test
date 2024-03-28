using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcLimitsClient;

var limit = 1_000;
var tasks = new List<Task>();
var sw = new Stopwatch();
sw.Start();
for (int i = 0; i < limit; i++)
{
    tasks.Add(SendRequest(i));
}

await Task.WhenAll(tasks);
sw.Stop();

Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
async Task SendRequest(int i)
{
    var sw = new Stopwatch();
    Console.WriteLine($"Sending request {i}");
    using var channel = GrpcChannel.ForAddress("https://grpc-limits-test.ambitiousground-e05aae5f.eastus.azurecontainerapps.io", new GrpcChannelOptions
    // using var channel = GrpcChannel.ForAddress("http://localhost:8080", new GrpcChannelOptions
    {
        HttpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay = TimeSpan.FromSeconds(5),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
            EnableMultipleHttp2Connections = true,
        }
    });
    var client = new Greeter.GreeterClient(channel);
    sw.Start();
    var response = client.SayHello();
    var tasks = new List<Task>();
    for (int j = 0; j < 10; j++)
    {
        await response.RequestStream.WriteAsync(new HelloRequest { Name = $"Client {i}" });
    }

    await foreach (var reply in response.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Response {i}: {reply.Message}");
    }
    sw.Stop();
}
