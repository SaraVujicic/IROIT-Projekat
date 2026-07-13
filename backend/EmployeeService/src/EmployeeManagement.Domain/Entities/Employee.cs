using EmployeeManagement.Domain.Exceptions;
using EmployeeManagement.Domain.ValueObjects;

namespace EmployeeManagement.Domain.Entities;

public class Employee
{
    public int Id { get; protected set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public CredentialsVO Credentials { get; private set; }
    public string Phone { get; private set; }
    public string Department { get; private set; } = "General";
    public int? ManagerId { get; private set; }
    
    // Navigation property for EF Core
    public virtual Employee? Manager { get; private set; }

    protected Employee() { } // For EF Core

    public Employee(string firstName, string lastName, CredentialsVO credentials, string phone, string department = "General")
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(department)) throw new DomainException("Department is required.");
        
        FirstName = firstName;
        LastName = lastName;
        Credentials = credentials ?? throw new DomainException("Credentials are required.");
        Phone = phone;
        Department = department;
    }

    public void UpdateDetails(string firstName, string lastName, string phone, string department = "General")
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(department)) throw new DomainException("Department is required.");

        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Department = department;
    }

    public void ChangeCredentials(CredentialsVO newCredentials)
    {
        Credentials = newCredentials ?? throw new DomainException("Credentials are required.");
    }

    public void AssignManager(int managerId)
    {
        if (managerId == Id)
        {
            throw new DomainException("An employee cannot be their own manager.");
        }
        ManagerId = managerId;
    }
    
    public void RemoveManager()
    {
        ManagerId = null;
    }
}
