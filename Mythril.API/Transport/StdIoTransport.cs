namespace Mythril.API.Transport;

public class StdIoTransport(TextReader input, TextWriter output) : ICommandTransport
{
    private readonly TextReader _input = input;
    private readonly TextWriter _output = output;

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        await _output.WriteLineAsync(message);
        await _output.FlushAsync(cancellationToken);
    }

    public async Task<string> ReceiveAsync(CancellationToken cancellationToken = default) => await _input.ReadLineAsync(cancellationToken) ?? string.Empty;
}
