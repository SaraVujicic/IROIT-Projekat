namespace DepartmentService.Domain.Entities;

using DepartmentService.Domain.Exceptions;

public class Department
{
    public int DepartmentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Department() { } // Required for EF Core

    public static Department CreateDepartment(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Department name is required and cannot be empty.");

        if (description != null && description.Length > 500)
            throw new DomainException("Description cannot exceed 500 characters.");

        return new Department
        {
            Name = name,
            Description = description
        };
    }

    public void ValidateUniqueName(bool isUniqueName)
    {
        if (!isUniqueName)
            throw new DomainException($"Department name '{Name}' already exists.");
    }

    public void UpdateDepartment(string name, string? description)
    {
        ChangeName(name);

        if (description != null && description.Length > 500)
            throw new DomainException("Description cannot exceed 500 characters.");
            
        Description = description;
    }

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Department name is required and cannot be empty.");
            
        Name = newName;
    }
}
