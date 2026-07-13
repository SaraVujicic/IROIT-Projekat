using RequestService.Domain.Common;

namespace RequestService.Domain.Entities;

public class TravelOrder : Entity
{
    public int TravelOrderId { get; private set; } // Specific requested structure, though Entity has Id
    public string Destination { get; private set; }
    public double Costs { get; private set; }

    private TravelOrder() { } // EF Core

    public TravelOrder(string destination, double costs)
    {
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination cannot be empty");
        if (costs < 0)
            throw new ArgumentException("Costs cannot be negative");

        Destination = destination;
        Costs = costs;
    }
}
