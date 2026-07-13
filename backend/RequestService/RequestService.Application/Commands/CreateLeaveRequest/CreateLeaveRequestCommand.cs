using MediatR;
using RequestService.Application.DTOs;
using RequestService.Domain.Entities;
using RequestService.Domain.Enums;
using RequestService.Domain.Interfaces;
using RequestService.Domain.ValueObjects;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace RequestService.Application.Commands.CreateLeaveRequest;

public record CreateLeaveRequestCommand(
    int EmployeeId,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    LeaveType? LeaveType,
    RequestType Type = RequestType.Leave,
    string? Destination = null,
    double? Costs = null,
    string? Name = null,
    string? MentorFirstName = null,
    string? MentorLastName = null,
    string? MentorPosition = null,
    bool Certificate = false) : IRequest<int>;

public class CreateLeaveRequestHandler : IRequestHandler<CreateLeaveRequestCommand, int>
{
    private readonly IRequestRepository _requestRepository;
    private readonly RequestService.Application.Services.IEmployeeServiceClient _employeeServiceClient;
    private readonly RequestService.Application.Interfaces.IMessageBus _messageBus;

    public CreateLeaveRequestHandler(
        IRequestRepository requestRepository, 
        RequestService.Application.Services.IEmployeeServiceClient employeeServiceClient,
        RequestService.Application.Interfaces.IMessageBus messageBus)
    {
        _requestRepository = requestRepository;
        _employeeServiceClient = employeeServiceClient;
        _messageBus = messageBus;
    }

    public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Synchronous inter-service communication (REST HTTP)
        var employeeExists = await _employeeServiceClient.ValidateEmployeeExistsAsync(request.EmployeeId, cancellationToken);
        if (!employeeExists)
        {
            throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} does not exist in EmployeeService.");
        }
        
        // 2. Check for overlapping dates logic
        var existingRequests = await _requestRepository.GetRequestsByEmployeeIdAsync(request.EmployeeId, cancellationToken);
        var hasOverlap = existingRequests.Any(r => 
            r.StartDate <= request.EndDate && r.EndDate >= request.StartDate && r.Status.Name != "Rejected");
            
        if (hasOverlap)
        {
            throw new InvalidOperationException("Cannot create request with overlapping dates.");
        }

        // 3. Submit request based on Type
        Request entity;
        if (request.Type == RequestType.Leave)
        {
            entity = Request.SubmitLeaveRequest(
                request.EmployeeId, 
                request.Description, 
                request.StartDate, 
                request.EndDate, 
                request.LeaveType ?? LeaveType.AnnualLeave);
        }
        else if (request.Type == RequestType.TravelOrder)
        {
            var travelOrder = new TravelOrder(request.Destination ?? "Default Destination", request.Costs ?? 0.0);
            entity = Request.SubmitTravelOrder(
                request.EmployeeId,
                request.Description,
                request.StartDate,
                request.EndDate,
                travelOrder);
        }
        else if (request.Type == RequestType.Internship)
        {
            var mentor = new MentorVO(
                request.MentorFirstName ?? "Default",
                request.MentorLastName ?? "Mentor",
                request.MentorPosition ?? "Manager");
            var internship = new Internship(
                request.Name ?? "Default Internship",
                mentor,
                request.StartDate,
                request.EndDate);
            entity = Request.SubmitInternship(
                request.EmployeeId,
                request.Description,
                request.StartDate,
                request.EndDate,
                internship);
        }
        else if (request.Type == RequestType.Education)
        {
            var education = new Education(
                request.Name ?? "Default Education",
                request.StartDate,
                request.EndDate,
                request.Certificate);
            entity = Request.SubmitEducation(
                request.EmployeeId,
                request.Description,
                request.StartDate,
                request.EndDate,
                education);
        }
        else
        {
            throw new ArgumentException("Invalid RequestType");
        }

        await _requestRepository.AddAsync(entity, cancellationToken);
        await _requestRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var createdEvent = new Events.RequestCreatedEvent
            {
                RequestId = entity.Id,
                EmployeeId = entity.EmployeeId,
                Description = entity.Description ?? string.Empty,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                RequestType = request.Type.ToString()
            };
            await _messageBus.PublishAsync(createdEvent, "request.created", cancellationToken);
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to publish RequestCreatedEvent for Request {RequestId}", entity.Id);
        }

        return entity.Id;
    }
}
