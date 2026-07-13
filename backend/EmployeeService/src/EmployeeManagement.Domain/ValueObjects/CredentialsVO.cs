namespace EmployeeManagement.Domain.ValueObjects;

public record CredentialsVO
{
    public string Username { get; init; }
    public string PasswordHash { get; init; }

    public CredentialsVO(string username, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password cannot be empty", nameof(passwordHash));

        Username = username;
        PasswordHash = passwordHash;
    }
}
