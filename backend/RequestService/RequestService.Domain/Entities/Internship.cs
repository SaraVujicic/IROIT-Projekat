using RequestService.Domain.Common;
using RequestService.Domain.ValueObjects;

namespace RequestService.Domain.Entities;

public class Internship : Entity
{
    public int InternshipId { get; private set; }
    public string Name { get; private set; }
    public MentorVO Mentor { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private Internship() { }

    public Internship(string name, MentorVO mentor, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty");
        if (startDate > endDate)
            throw new ArgumentException("StartDate cannot be after EndDate");

        Name = name;
        Mentor = mentor ?? throw new ArgumentNullException(nameof(mentor));
        StartDate = startDate;
        EndDate = endDate;
    }
}
