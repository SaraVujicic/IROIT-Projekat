using RequestService.Domain.Common;

namespace RequestService.Domain.ValueObjects;

public class MentorVO : ValueObject
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Position { get; private set; }

    private MentorVO() { }

    public MentorVO(string firstName, string lastName, string position)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty");
            
        FirstName = firstName;
        LastName = lastName;
        Position = position;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Position;
    }
}
