// See https://aka.ms/new-console-template for more information

using Wyzarzanie;

class Program
{
    static void Main(string[] args)
    {
        SimulatedAnnealing sa = new SimulatedAnnealing();

        Func<double, double> quadraticFunction = x => x * x - 4 * x + 3;

        double xMin = -10;
        double xMax = 10;
        double initialTemperature = 1000;
        double coolingRate = 0.99;
        double minTemperature = 0.01;

        double minX = sa.Minimize(quadraticFunction, xMin, xMax, initialTemperature, coolingRate, minTemperature);

        Console.WriteLine($"Minimum found at x = {minX}, f(x) = {quadraticFunction(minX)}");
    }
}