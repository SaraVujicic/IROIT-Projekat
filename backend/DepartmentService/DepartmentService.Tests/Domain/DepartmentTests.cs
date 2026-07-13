namespace DepartmentService.Tests.Domain;

using DepartmentService.Domain.Entities;
using DepartmentService.Domain.Exceptions;
using FluentAssertions;
using Xunit;

public class DepartmentTests
{
    [Fact]
    public void CreateDepartment_Should_Succeed_When_Valid()
    {
        var dept = Department.CreateDepartment("Engineering", "Tech department");
        
        dept.Name.Should().Be("Engineering");
        dept.Description.Should().Be("Tech department");
    }

    [Fact]
    public void CreateDepartment_Should_ThrowDomainException_When_NameIsInvalid()
    {
        var act = () => Department.CreateDepartment("", "Desc");
        act.Should().Throw<DomainException>();
    }

}
