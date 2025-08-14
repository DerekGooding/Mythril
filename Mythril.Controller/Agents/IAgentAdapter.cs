using System.Threading.Tasks;

namespace Mythril.Controller.Agents
{
    public interface IAgentAdapter
    {
        Task<string[]> GetCommandsAsync(string prompt);
        Task SendResponseAsync(string response);
        Task SendImageAsync(byte[] imageData);
    }
}
