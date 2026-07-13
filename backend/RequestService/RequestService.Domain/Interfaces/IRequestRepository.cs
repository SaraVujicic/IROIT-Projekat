using RequestService.Domain.Entities;

namespace RequestService.Domain.Interfaces;

public interface IRequestRepository
{
    IUnitOfWork UnitOfWork { get; }
    
    Task<Request> AddAsync(Request request, CancellationToken cancellationToken = default);
    Task<Request?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetRequestsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    void Update(Request request);
    void Delete(Request request);
}
