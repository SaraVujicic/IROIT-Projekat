using AutoMapper;
using MediatR;
using RequestService.Application.DTOs;
using RequestService.Domain.Interfaces;

namespace RequestService.Application.Queries.GetEmployeeRequests;

public record GetEmployeeRequestsQuery(int EmployeeId) : IRequest<IEnumerable<RequestDto>>;

public class GetEmployeeRequestsHandler : IRequestHandler<GetEmployeeRequestsQuery, IEnumerable<RequestDto>>
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMapper _mapper;

    public GetEmployeeRequestsHandler(IRequestRepository requestRepository, IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RequestDto>> Handle(GetEmployeeRequestsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _requestRepository.GetRequestsByEmployeeIdAsync(request.EmployeeId, cancellationToken);
        return _mapper.Map<IEnumerable<RequestDto>>(entities);
    }
}

public record GetAllRequestsQuery() : IRequest<IEnumerable<RequestDto>>;

public class GetAllRequestsHandler : IRequestHandler<GetAllRequestsQuery, IEnumerable<RequestDto>>
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMapper _mapper;

    public GetAllRequestsHandler(IRequestRepository requestRepository, IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RequestDto>> Handle(GetAllRequestsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _requestRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<RequestDto>>(entities);
    }
}
