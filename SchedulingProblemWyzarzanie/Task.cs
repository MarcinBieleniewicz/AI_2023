using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulingProblemWyzarzanie
{
    //klasa ktora ulatwia trzymanie wlasciwosci zadan w jednym miejscu
    internal class Task
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public List<Task> Dependencies { get; set; }

        public Task(string name, int duration)
        {
            Name = name;
            Duration = duration;
            Dependencies = new List<Task>();
        }
    }
}
