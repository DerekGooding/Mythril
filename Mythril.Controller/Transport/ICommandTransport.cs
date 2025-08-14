namespace Mythril.Controller.Transport;

public interface ICommandTransport
{
    Task SendAsync(string message, CancellationToken cancellationToken = default);
    Task<string> ReceiveAsync(CancellationToken cancellationToken = default);
}
