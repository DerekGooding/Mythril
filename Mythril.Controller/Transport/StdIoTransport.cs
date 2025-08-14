using Mythril.API.Transport;

namespace Mythril.Controller.Transport;

public class StdIoTransport : ICommandTransport
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public StdIoTransport()
    {
        _input = Console.In;
        _output = Console.Out;
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        await _output.WriteLineAsync(message);
        await _output.FlushAsync();
    }

    public async Task<string> ReceiveAsync(CancellationToken cancellationToken = default) => await _input.ReadLineAsync() ?? string.Empty;
}
