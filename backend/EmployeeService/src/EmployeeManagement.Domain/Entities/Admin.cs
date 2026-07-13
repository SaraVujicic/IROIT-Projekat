using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class Admin : Employee
{
    protected Admin() : base() { }

    public Admin(string firstName, string lastName, CredentialsVO credentials, string phone, string department = "Administration")
        : base(firstName, lastName, credentials, phone, department)
    {
    }
}
