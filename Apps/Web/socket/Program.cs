using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.UseWebSockets();

var _factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "admin",
    Password = "admin",
};
app.Map(
    "/transmit",
    async context =>
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "send-chunk",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        try
        {
            WebSocketReceiveResult result;
            do
            {
                var buffer = new byte[1024 * 50];
                using var memoryStream = new MemoryStream();
                do
                {
                    result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None
                    );
                    await memoryStream.WriteAsync(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var content = JsonConvert.SerializeObject(new { Chunk = memoryStream.ToArray() });

                var body = Encoding.UTF8.GetBytes(content);

                Console.WriteLine("send chunk");

                channel.BasicPublish(
                    exchange: "",
                    routingKey: "send-chunk",
                    basicProperties: null,
                    body: body
                );
            } while (!result.CloseStatus.HasValue);
        }
        catch (WebSocketException)
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.InternalServerError,
                "WebSocket error occurred.",
                CancellationToken.None
            );
        }
    }
);

app.Run();
