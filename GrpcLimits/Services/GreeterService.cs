using Grpc.Core;
using GrpcLimits;

namespace GrpcLimits.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override async Task SayHello(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            _logger.LogInformation("Received: {Name}", request.Name);
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(1000);
                await responseStream.WriteAsync(new HelloReply { Message = $"Hello {request.Name} - {i}" });
            }
        }
    }
}
