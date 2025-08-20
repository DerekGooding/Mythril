using Mythril.API;

namespace Mythril.GameLogic.AI;

public interface ICommandExecutor
{
    Task ExecuteCommand(Command command);
}
