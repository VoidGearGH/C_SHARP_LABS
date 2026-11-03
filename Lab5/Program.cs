using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
public class PassengerTrain
{
    public PassengerTrain()
    {

    }

    private List<PassengerCarriage> _carriages = new List<PassengerCarriage>();
    public required Locomotive Locomotive { get; init; }
    public int CountPassengers() => _carriages.Select(c => c.NumberOfPassengers).Sum();
    public decimal CountBaggage() => _carriages.Select(c => c.BaggageWeight).Sum();
    public void SortCarriagesByComfort() => _carriages = _carriages.OrderBy(c => c.ComfortLevel).ToList();
    public List<PassengerCarriage> GetCarriagesByRange(int min, int max) => _carriages.Where(c => c.NumberOfPassengers >= min && c.NumberOfPassengers <= max).ToList();
}
public abstract class RollingStock
{
    public required string Name { get; init; }
    public required decimal Mass { get; init; }
}
public abstract class Carriage : RollingStock
{
    public required int Capacity { get; init; }
    public required int NumberOfPassengers { get; set; }
    public required decimal BaggageWeight { get; init; }
}
public class Locomotive : RollingStock
{
    public required decimal Power { get; init; }
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
}
public class CompartmentCarriage : PassengerCarriage
{

}
public class Open_SleeperCar : PassengerCarriage
{

}
public class SV_Carriage : PassengerCarriage
{

}
public class LuxuryCarriage : PassengerCarriage
{

}
internal class Program
{
    static void Main()
    {
            
    }
}