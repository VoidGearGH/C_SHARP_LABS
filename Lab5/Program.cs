using System;
using System.Collections.Generic;
public class PassengerTrain
{
    private List<PassengerCarriage> _carriages = new List<PassengerCarriage>();
    public required Locomotive Locomotive { get; init; }
}
public abstract class RollingStock
{
    public required string Name { get; init; }
    public required decimal Mass { get; init; }
}
public abstract class Carriage : RollingStock
{
    public required int Capacity { get; init; }
    public required int NumberOfPassengers { get; init; }
    public required decimal BaggageWeight { get; init; }
}
public class Locomotive : RollingStock
{
    public required decimal Power { get; init; }
}
public class PassengerCarriage : Carriage
{

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