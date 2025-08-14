using Mythril.API.Transport;
using Mythril.Controller.Transport;

ICommandTransport? transport = null;
if (args.Length > 0 && args[0] == "--transport")
{
    if (args.Length > 1 && args[1] == "stdio")
    {
        transport = new StdIoTransport();
    }
}

using var game = new Mythril.Game1(transport);
game.Run();