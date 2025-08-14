using System.Collections.Concurrent;
using Newtonsoft.Json;
using Mythril.API;
using Mythril.API.Transport;

namespace Mythril.GameLogic.AI;

public class CommandListener(ICommandTransport transport)
{
    private readonly ICommandTransport _transport = transport;
    private readonly ConcurrentQueue<Command> _commandQueue = new();
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public void StartListening() => Task.Run(async () => await ListenForCommands(_cancellationTokenSource.Token));

    public void StopListening() => _cancellationTokenSource.Cancel();

    private async Task ListenForCommands(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var jsonCommand = await _transport.ReceiveAsync(cancellationToken);
                if (!string.IsNullOrEmpty(jsonCommand))
                {
                    var command = JsonConvert.DeserializeObject<Command>(jsonCommand);
                    if (command != null)
                        _commandQueue.Enqueue(command);
                }
            }
            catch (TaskCanceledException)
            {
                // Listening was cancelled
                break;
            }
            catch (Exception ex)
            {
                // Log error, but don't stop listening
                Console.WriteLine($"Error receiving command: {ex.Message}");
            }
        }
    }

    public bool TryDequeueCommand(out Command? command) => _commandQueue.TryDequeue(out command);
}
