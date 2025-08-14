using System.IO.Pipes;

namespace Mythril.Controller.Transport;

public class NamedPipeTransport : ICommandTransport
{
    private readonly PipeStream _pipeStream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public NamedPipeTransport(PipeStream stream)
    {
        _pipeStream = stream;
        _reader = new StreamReader(_pipeStream);
        _writer = new StreamWriter(_pipeStream);
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync(message);
        await _writer.FlushAsync();
    }

    public async Task<string> ReceiveAsync(CancellationToken cancellationToken = default) => await _reader.ReadLineAsync();
    public static async Task<NamedPipeTransport> CreateServer(string pipeName, CancellationToken cancellationToken = default)
    {
        var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        await pipeServer.WaitForConnectionAsync(cancellationToken);
        return new NamedPipeTransport(pipeServer);
    }

    public static NamedPipeTransport CreateClient(string pipeName)
    {
        var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        pipeClient.Connect();
        return new NamedPipeTransport(pipeClient);
    }
}