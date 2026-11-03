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

        carriage.ComfortLevel = cType switch
        {
            CarriageType.Compartment => Comfort.Basic,
            CarriageType.Open_Sleeper => Comfort.Basic,
            CarriageType.SV => Comfort.Comfort,
            CarriageType.Luxury => Comfort.Luxury,
            _ => Comfort.Basic
        };

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
        var locomotive = new Locomotive("Локомотив", 120m, 7200m);
        var train = new PassengerTrain(locomotive);

        AddCarriageToTrain(train, CarriageType.Compartment, "Купейный №1", 50m, 36, 15, 200m);
        AddCarriageToTrain(train, CarriageType.Open_Sleeper, "Плацкарт №1", 55m, 54, 45, 300m);
        AddCarriageToTrain(train, CarriageType.SV, "СВ №1", 60m, 18, 8, 150m);
        AddCarriageToTrain(train, CarriageType.Luxury, "Люкс №1", 65m, 12, 4, 100m);
        AddCarriageToTrain(train, CarriageType.Compartment, "Купейный №2", 50m, 36, 25, 250m);

        Console.WriteLine($"Всего пассажиров: {train.CountPassengers()}");
        Console.WriteLine($"Всего багажа: {train.CountBaggage()} кг\n");

        Console.WriteLine("Вагоны с количеством пассажиров [от 10 до 30]");
        var filtered = train.GetCarriagesByRange(10, 30);
        foreach (var c in filtered)
        {
            Console.WriteLine($"  {c.Name}: {c.NumberOfPassengers} пасс., комфорт: {c.ComfortLevel}");
        }
        Console.WriteLine();

        Console.WriteLine("После сортировки по уровню комфорта");
        train.SortCarriagesByComfort();
        foreach (var c in train.GetCarriagesByRange(0, 100))
        {
            Console.WriteLine($"  {c.Name} ({c.ComfortLevel}): {c.NumberOfPassengers} пасс.");
        }
    }

    private void AddCarriageToTrain(PassengerTrain train, CarriageType type, string name, decimal mass, int capacity, int passengers, decimal baggage)
    {
        var result = train.AddCarriage(type, name, mass, capacity, passengers, baggage);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Ошибка добавления вагона {name}: {result.ErrorMessage}");
        }
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