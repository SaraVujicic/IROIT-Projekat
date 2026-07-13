using System.Threading;
using System.Threading.Tasks;

namespace RequestService.Application.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
}
