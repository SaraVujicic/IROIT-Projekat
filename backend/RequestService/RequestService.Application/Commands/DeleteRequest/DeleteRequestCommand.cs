using MediatR;
using RequestService.Domain.Interfaces;

namespace RequestService.Application.Commands.DeleteRequest;

public record DeleteRequestCommand(int RequestId) : IRequest<bool>;

public class DeleteRequestHandler : IRequestHandler<DeleteRequestCommand, bool>
{
    private readonly IRequestRepository _requestRepository;

    public DeleteRequestHandler(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<bool> Handle(DeleteRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Request with ID {request.RequestId} not found.");

        _requestRepository.Delete(entity);
        await _requestRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
