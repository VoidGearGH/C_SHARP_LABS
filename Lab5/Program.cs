using System;
using System.Collections.Generic;
using System.Linq;

public class PassengerTrain
{
    private List<PassengerCarriage> _carriages = new List<PassengerCarriage>();
    public Locomotive Locomotive { get; init; }

    public PassengerTrain(Locomotive locomotive)
    {
        Locomotive = locomotive;
    }

    public int CountPassengers() => _carriages.Select(c => c.NumberOfPassengers).Sum();
    public decimal CountBaggage() => _carriages.Select(c => c.BaggageWeight).Sum();
    public void SortCarriagesByComfort() => _carriages = _carriages.OrderBy(c => c.ComfortLevel).ToList();
    public List<PassengerCarriage> GetCarriagesByRange(int min, int max) => _carriages.Where(c => c.NumberOfPassengers >= min && c.NumberOfPassengers <= max).ToList();

    public OperationResult AddCarriage(CarriageType cType, string name, decimal mass, int capacity, int numOfPasss, decimal baggWeight)
    {
        PassengerCarriage? carriage = cType switch
        {
            CarriageType.Compartment => new CompartmentCarriage(name, mass, capacity, numOfPasss, baggWeight),
            CarriageType.Open_Sleeper => new Open_SleeperCarriage(name, mass, capacity, numOfPasss, baggWeight),
            CarriageType.SV => new SV_Carriage(name, mass, capacity, numOfPasss, baggWeight),
            CarriageType.Luxury => new LuxuryCarriage(name, mass, capacity, numOfPasss, baggWeight),
            _ => null
        };

        if (carriage == null)
            return OperationResult.Failure("Wrong type");

        _carriages.Add(carriage);
        return OperationResult.Success();
    }
}

public record OperationResult(bool IsSuccess, string? ErrorMessage = null)
{
    public static OperationResult Success() => new(true, null);
    public static OperationResult Failure(string error) => new(false, error);
}

public enum CarriageType
{
    Compartment,
    Open_Sleeper,
    SV,
    Luxury
}

public abstract class RollingStock
{
    public string Name { get; init; }
    public decimal Mass { get; init; }

    protected RollingStock(string name, decimal mass)
    {
        Name = name;
        Mass = mass;
    }
}

public abstract class Carriage : RollingStock
{
    public int Capacity { get; init; }
    public int NumberOfPassengers { get; set; }
    public decimal BaggageWeight { get; init; }

    public Carriage(string name, decimal mass, int capacity, int numOfPassengers, decimal baggageWeight)
        : base(name, mass)
    {
        Capacity = capacity;
        NumberOfPassengers = numOfPassengers;
        BaggageWeight = baggageWeight;
    }
}

public class Locomotive : RollingStock
{
    public decimal Power { get; init; }

    public Locomotive(string name, decimal mass, decimal power) : base(name, mass)
    {
        Power = power;
    }
}

public enum Comfort
{
    Basic,
    Comfort,
    Luxury
}

public class PassengerCarriage : Carriage
{
    public Comfort ComfortLevel { get; set; }

    public PassengerCarriage(string name, decimal mass, int capacity, int numOfPassengers, decimal baggageWeight)
        : base(name, mass, capacity, numOfPassengers, baggageWeight) { }
}

public class CompartmentCarriage : PassengerCarriage
{
    public CompartmentCarriage(string name, decimal mass, int capacity, int numOfPassengers, decimal baggageWeight)
        : base(name, mass, capacity, numOfPassengers, baggageWeight) { }
}

public class Open_SleeperCarriage : PassengerCarriage
{
    public Open_SleeperCarriage(string name, decimal mass, int capacity, int numOfPassengers, decimal baggageWeight)
        : base(name, mass, capacity, numOfPassengers, baggageWeight) { }
}

public class SV_Carriage : PassengerCarriage
{
    public SV_Carriage(string name, decimal mass, int capacity, int numOfPassengers, decimal baggageWeight)
        : base(name, mass, capacity, numOfPassengers, baggageWeight) { }
}

public class LuxuryCarriage : PassengerCarriage
{
    public LuxuryCarriage(string name, decimal mass, int capacity, int numOfPassengers, decimal baggageWeight)
        : base(name, mass, capacity, numOfPassengers, baggageWeight) { }
}

public class Interaction
{
    public void Start()
    {
        var locomotive = new Locomotive("ЭП20", 120m, 7200m);
        var train = new PassengerTrain(locomotive);
        train.AddCarriage(CarriageType.Compartment, "Купейный 1", 50m, 36, 15, 200m);
    }
}

internal class Program
{
    static void Main()
    {
        Interaction interaction = new();
        interaction.Start();
    }
}