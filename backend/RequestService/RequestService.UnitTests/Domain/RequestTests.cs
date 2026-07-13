using RequestService.Domain.Entities;
using RequestService.Domain.Enums;
using RequestService.Domain.ValueObjects;
using Xunit;

namespace RequestService.UnitTests.Domain;

public class RequestTests
{
    [Fact]
    public void SubmitLeaveRequest_WithValidData_ShouldCreatePendingRequest()
    {
        // Arrange
        int employeeId = 1;
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);
        var type = LeaveType.AnnualLeave;

        // Act
        var request = Request.SubmitLeaveRequest(employeeId, "Vacation", startDate, endDate, type);

        // Assert
        Assert.Equal(employeeId, request.EmployeeId);
        Assert.Equal(StatusVO.Pending, request.Status);
        Assert.Equal(RequestType.Leave, request.Type);
    }

    [Fact]
    public void ApproveByManager_WhenPending_ShouldChangeToPendingAdminApproval()
    {
        // Arrange
        var request = Request.SubmitLeaveRequest(1, "Vacation", DateTime.Today.AddDays(1), DateTime.Today.AddDays(5), LeaveType.AnnualLeave);
        var audit = new ManagerCredentialsVO("Manager", "Password");

        // Act
        request.ApproveByManager(audit);

        // Assert
        Assert.Equal(StatusVO.PendingAdminApproval, request.Status);
    }
}
