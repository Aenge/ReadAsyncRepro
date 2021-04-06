using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace ReadAsyncRepro
{
    class Handler : ConnectionHandler
    {
        private readonly ILogger<Handler> _logger;
        public Handler(ILogger<Handler> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            ProtocolReader protocolReader = connection.CreateReader();
            CustomProtocol customProtocol = new();

            string message = "first\n";
            Memory<byte> buffer = connection.Transport.Output.GetMemory(0x10)[..0x10];
            System.Text.Encoding.UTF8.GetBytes(message).CopyTo(buffer);
            
            connection.Transport.Output.Advance(message.Length);
            await connection.Transport.Output.FlushAsync();

            var result = await protocolReader.ReadAsync(customProtocol);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(result.Message.ToArray()));

            protocolReader.Advance();

            message = "second\n";
            while (true)
            {
                try
                {
                    result = await protocolReader.ReadAsync(customProtocol, new CancellationTokenSource(2000).Token);
                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(result.Message.ToArray()));
                    protocolReader.Advance();
                }
                catch (OperationCanceledException exception)
                {
                    buffer = connection.Transport.Output.GetMemory(0x10)[..0x10];
                    System.Text.Encoding.UTF8.GetBytes(message).CopyTo(buffer);
                    connection.Transport.Output.Advance(message.Length);
                    await connection.Transport.Output.FlushAsync();
                }
                catch (Exception a)
                {
                    Console.WriteLine(a);
                    return;
                }
            }
        }
    }
}
