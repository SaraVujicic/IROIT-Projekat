using MediatR;
using RequestService.Domain.Interfaces;
using Serilog;

namespace RequestService.Application.Commands.ApproveRequest;

public record ApproveRequestCommand(int RequestId, string ApproverRole, bool Approve = true) : IRequest<bool>;

public class ApproveRequestHandler : IRequestHandler<ApproveRequestCommand, bool>
{
    private readonly IRequestRepository _requestRepository;
    private readonly RequestService.Application.Interfaces.IMessageBus _messageBus;

    public ApproveRequestHandler(
        IRequestRepository requestRepository,
        RequestService.Application.Interfaces.IMessageBus messageBus)
    {
        _requestRepository = requestRepository;
        _messageBus = messageBus;
    }

    public async Task<bool> Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Request with ID {request.RequestId} not found.");

        if (!request.Approve)
        {
            entity.Reject();
        }
        else if (request.ApproverRole == "Manager")
        {
            var managerAudit = new RequestService.Domain.ValueObjects.ManagerCredentialsVO("SystemManager", "AuditLog");
            entity.ApproveByManager(managerAudit);
        }
        else if (request.ApproverRole == "Admin")
        {
            var adminAudit = new RequestService.Domain.ValueObjects.AdminCredentialsVO("SystemAdmin", "AuditLog");
            entity.ApproveByAdmin(adminAudit);
        }
        else
        {
            throw new UnauthorizedAccessException("Only managers and admins can approve requests.");
        }

        _requestRepository.Update(entity);
        await _requestRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var statusChangedEvent = new Events.RequestStatusChangedEvent
            {
                RequestId = entity.Id,
                EmployeeId = entity.EmployeeId,
                NewStatus = entity.Status?.Name ?? "Unknown",
                ApproverRole = request.ApproverRole,
                Description = entity.Description ?? string.Empty
            };
            await _messageBus.PublishAsync(statusChangedEvent, "request.statuschanged", cancellationToken);
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to publish RequestStatusChangedEvent for Request {RequestId}", entity.Id);
        }

        return true;
    }
}
