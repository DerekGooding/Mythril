using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Mythril.Controller.Transport;

namespace Mythril.GameLogic.AI
{
    public class CommandListener
    {
        private readonly ICommandTransport _transport;
        private readonly ConcurrentQueue<Command> _commandQueue;
        private CancellationTokenSource _cancellationTokenSource;

        public CommandListener(ICommandTransport transport)
        {
            _transport = transport;
            _commandQueue = new ConcurrentQueue<Command>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartListening()
        {
            Task.Run(async () => await ListenForCommands(_cancellationTokenSource.Token));
        }

        public void StopListening()
        {
            _cancellationTokenSource.Cancel();
        }

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
                        {
                            _commandQueue.Enqueue(command);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Listening was cancelled
                    break;
                }
                catch (System.Exception ex)
                {
                    // Log error, but don't stop listening
                    Console.WriteLine($"Error receiving command: {ex.Message}");
                }
            }
        }

        public bool TryDequeueCommand(out Command command)
        {
            return _commandQueue.TryDequeue(out command);
        }
    }
}
