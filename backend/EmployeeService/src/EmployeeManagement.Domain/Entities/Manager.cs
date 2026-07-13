using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class Manager : Employee
{
    protected Manager() : base() { }

    public Manager(string firstName, string lastName, CredentialsVO credentials, string phone, string department = "Management")
        : base(firstName, lastName, credentials, phone, department)
    {
    }
}
