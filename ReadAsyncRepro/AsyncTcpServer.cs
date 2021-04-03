using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ReadAsyncRepro
{
    public class AsyncTcpServer<T> where T : ConnectionHandler
    {
        public ServiceCollection serviceCollection { get; }
        public ServiceProvider serviceProvider { get; }
        public Server server { get; }
        ILogger<T> logger { get; }
        public AsyncTcpServer(ushort port)
        {
            serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });

            serviceCollection.AddSignalR();

            serviceProvider = serviceCollection.BuildServiceProvider();

            server = new ServerBuilder(serviceProvider)
                        .UseSockets(sockets =>
                        {
                            sockets.ListenAnyIP(port,
                                builder => {
                                    builder.UseConnectionLogging().UseConnectionHandler<T>();
                                });
                        })
                        .Build();

            logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();
        }
        public async Task run()
        {
            await server.StartAsync();
            foreach (var ep in server.EndPoints)
            {
                logger.LogInformation("Listening on {EndPoint}", ep);
            }
            var tcs = new TaskCompletionSource<object>();
            await tcs.Task;

            await server.StopAsync();

        }
        
    }
}
