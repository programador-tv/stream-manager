using System.Runtime.Intrinsics.Arm;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace manager;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private ProcessManager SingleProcess;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "admin",
            Password = "admin",
        };
        var connection = _factory.CreateConnection();
        var channel = connection.CreateModel();

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
             
                var chunkStruct = 
                    JsonConvert.DeserializeObject<ChunkStruct>(message);
                
                SingleProcess ??= new ProcessManager("single");
                SingleProcess.WriteToStandardInput(chunkStruct.Chunk);
                
            };

        channel.BasicConsume(queue: "send-chunk", autoAck: true, consumer: consumer);

        consumer.Shutdown += (model, ea) =>
        {
            channel.Dispose();
        };

    }
}
