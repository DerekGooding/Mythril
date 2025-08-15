using Mythril.API;
using System.Threading.Tasks;

namespace Mythril.GameLogic.AI
{
    public interface ICommandExecutor
    {
        Task ExecuteCommand(Command command);
    }
}
