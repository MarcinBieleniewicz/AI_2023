using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyzarzanie
{
    internal class SimulatedAnnealing
    {
        private readonly Random _random = new Random();

        public double Minimize(Func<double, double> function, double xMin, double xMax, double initialTemperature, double coolingRate, double minTemperature)
        {
            double currentX = _random.NextDouble() * (xMax - xMin) + xMin;
            double bestX = currentX;
            double currentEnergy = function(currentX);
            double bestEnergy = currentEnergy;
            double temperature = initialTemperature;

            while (temperature > minTemperature)
            {
                double newX = _random.NextDouble() * (xMax - xMin) + xMin;
                double newEnergy = function(newX);
                double deltaEnergy = newEnergy - currentEnergy;

                if (deltaEnergy < 0 || Math.Exp(-deltaEnergy / temperature) > _random.NextDouble())
                {
                    currentX = newX;
                    currentEnergy = newEnergy;

                    if (currentEnergy < bestEnergy)
                    {
                        bestX = currentX;
                        bestEnergy = currentEnergy;
                    }
                }

                temperature *= coolingRate;
            }

            return bestX;
        }
    }
}
