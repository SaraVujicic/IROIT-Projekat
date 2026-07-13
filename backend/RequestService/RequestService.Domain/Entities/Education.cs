using RequestService.Domain.Common;

namespace RequestService.Domain.Entities;

public class Education : Entity
{
    public int EducationId { get; private set; }
    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool Certificate { get; private set; }

    private Education() { }

    public Education(string name, DateTime startDate, DateTime endDate, bool certificate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty");
        if (startDate > endDate)
            throw new ArgumentException("StartDate cannot be after EndDate");

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Certificate = certificate;
    }
}
