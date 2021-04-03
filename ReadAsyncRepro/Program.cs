using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ReadAsyncRepro
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TcpClient client = new();
            AsyncTcpServer<Handler> tcpServer = new(54000);
            await Task.WhenAny(tcpServer.run(), doClient(client));
        }

        static async Task doClient(TcpClient client)
        {
            byte[] buffer = new byte[0x100];
            await client.ConnectAsync("localhost", 54000);
            using NetworkStream networkStream = client.GetStream();

            while (true)
            {
                int size = await networkStream.ReadAsync(buffer);
                if (size == 0)
                    return;
                await networkStream.WriteAsync(buffer[..size]);
            }
        }
    }
}
