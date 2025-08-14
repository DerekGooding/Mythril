using System.Net;
using System.Net.Sockets;

namespace Mythril.Controller.Transport;

public class TcpTransport : ICommandTransport
{
    private readonly TcpClient _tcpClient;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public TcpTransport(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        var stream = _tcpClient.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream);
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync(message);
        await _writer.FlushAsync();
    }

    public async Task<string> ReceiveAsync(CancellationToken cancellationToken = default) => await _reader.ReadLineAsync();

    public static async Task<TcpTransport> CreateServer(int port, CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        var client = await listener.AcceptTcpClientAsync(cancellationToken);
        return new TcpTransport(client);
    }

    public static TcpTransport CreateClient(int port)
    {
        var client = new TcpClient();
        client.Connect(IPAddress.Loopback, port);
        return new TcpTransport(client);
    }
}
