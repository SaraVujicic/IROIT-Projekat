using Microsoft.EntityFrameworkCore;
using RequestService.Domain.Entities;
using RequestService.Domain.Interfaces;
using RequestService.Infrastructure.Data;

namespace RequestService.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly RequestDbContext _context;

    public RequestRepository(RequestDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Request> AddAsync(Request request, CancellationToken cancellationToken = default)
    {
        await _context.Requests.AddAsync(request, cancellationToken);
        return request;
    }

    public async Task<Request?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.TravelOrders)
            .Include(r => r.Internships)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.TravelOrders)
            .Include(r => r.Internships)
            .Include(r => r.Educations)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        // Actually checking logic, Status Name will map to DB String
        return await _context.Requests
            .Include(r => r.TravelOrders)
            .Include(r => r.Internships)
            .Include(r => r.Educations)
            // Just comparing Name because we don't have enum mapping for StatusVO
            .Where(r => r.Status.Name == "Pending" || r.Status.Name == "PendingAdminApproval")
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetRequestsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.TravelOrders)
            .Include(r => r.Internships)
            .Include(r => r.Educations)
            .Where(r => r.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);
    }

    public void Update(Request request)
    {
        _context.Entry(request).State = EntityState.Modified;
    }

    public void Delete(Request request)
    {
        _context.Requests.Remove(request);
    }
}
