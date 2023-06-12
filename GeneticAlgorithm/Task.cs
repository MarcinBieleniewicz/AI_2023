using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithmSolution
{
    //klasa ktora ulatwia trzymanie wlasciwosci zadan w jednym miejscu
    class Task
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public List<Task> Dependencies { get; set; }
        public int Processor { get; set; }

        public Task(string name, int duration)
        {
            Name = name;
            Duration = duration;
            Dependencies = new List<Task>();
            Processor = 0;
        }
    }
}
