using System.Threading;
using System.Threading.Tasks;

namespace Mythril.Controller.Transport
{
    public interface ICommandTransport
    {
        Task SendAsync(string message, CancellationToken cancellationToken = default);
        Task<string> ReceiveAsync(CancellationToken cancellationToken = default);
    }
}
