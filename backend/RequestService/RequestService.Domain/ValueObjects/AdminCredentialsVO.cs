using RequestService.Domain.Common;

namespace RequestService.Domain.ValueObjects;

public class AdminCredentialsVO : ValueObject
{
    public string Username { get; private set; }
    public string Password { get; private set; } // Hashed password in real system

    private AdminCredentialsVO() { }

    public AdminCredentialsVO(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");

        Username = username;
        Password = password;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Username;
        yield return Password;
    }
}
