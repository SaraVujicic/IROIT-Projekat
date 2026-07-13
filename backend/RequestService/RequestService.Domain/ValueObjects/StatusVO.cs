using RequestService.Domain.Common;

namespace RequestService.Domain.ValueObjects;

public class StatusVO : ValueObject
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    private StatusVO() { } // For EF Core

    private StatusVO(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public static StatusVO Pending => new StatusVO("Pending", "Request is pending approval");
    public static StatusVO PendingAdminApproval => new StatusVO("PendingAdminApproval", "Request was approved by manager, pending admin");
    public static StatusVO Approved => new StatusVO("Approved", "Request is approved");
    public static StatusVO Rejected => new StatusVO("Rejected", "Request is rejected");

    public static StatusVO Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Status name cannot be empty", nameof(name));
            
        return new StatusVO(name, description);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
