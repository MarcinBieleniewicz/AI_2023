using System;
using System.Collections.Generic;
using GAF;
using GAF.Extensions;
using GAF.Operators;
using GAF.Threading;

namespace GeneticAlgorithmSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            var tasks = GetTasksFromUser();
            var numberOfProcessors = GetNumberOfProcessorsFromUser();

            //parametry algorytmu genetycznego
            var populationSize = 200;
            var crossoverProbability = 0.8;
            var mutationProbability = 0.5;
            var elitismPercentage = 20;

            //przetrzymuje rozwiazanie
            var population = new Population();

            for (int i = 0; i < populationSize; i++)
            {
                var solution = GetRandomSolution(tasks, numberOfProcessors);
                var chromosome = new Chromosome();
                foreach (var task in solution)
                {
                    var gene = new Gene(task);
                    chromosome.Genes.Add(gene);
                }
                population.Solutions.Add(chromosome);
            }

            var ga = new GeneticAlgorithm(population, CalculateFitness);

            ga.OnGenerationComplete += ga_OnGenerationComplete;

            var elite = new Elite(elitismPercentage);
            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };
            var mutation = new SwapMutate(mutationProbability);

            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            ga.Run(TerminateAlgorithm);

            var bestSolution = population.Solutions.OrderByDescending(chromosome => CalculateFitness(chromosome)).First();
            var bestSchedule = DecodeSolution(bestSolution.Genes.Select(gene => (Task)gene.ObjectValue).ToList(), numberOfProcessors);

            Console.WriteLine("Best Schedule:");
            PrintSchedule(bestSchedule);
        }

        //wyswietla numer pokolenia oraz optymalnosc
        static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var bestFitness = e.Population.MaximumFitness;
            Console.WriteLine($"Generation: {e.Generation} Best Fitness: {bestFitness}");
        }

        //Metoda obliczajaca optymalnosc chromosomu
        static double CalculateFitness(Chromosome chromosome)
        {
            var solution = chromosome.Genes.Select(gene => (Task)gene.ObjectValue).ToList();
            var numberOfProcessors = solution.Max(t => t.Processor) + 1;

            var schedule = DecodeSolution(solution, numberOfProcessors);
            var makespan = CalculateMakespan(schedule);

            return 1.0 / makespan;
        }

        //zamienia liste na rozwiazanie
        static List<List<Task>> DecodeSolution(List<Task> solution, int numberOfProcessors)
        {
            var schedule = new List<List<Task>>();

            for (int i = 0; i < numberOfProcessors; i++)
            {
                schedule.Add(new List<Task>());
            }

            foreach (var task in solution)
            {
                schedule[task.Processor].Add(task);
            }

            return schedule;
        }

        //wygeneruj losowe rozwiazanie na podstawie nieprzypisanych zadan uwzgledniajac ich zaleznosci
        static List<Task> GetRandomSolution(List<Task> tasks, int numberOfProcessors)
        {
            var random = new Random();
            var solution = new List<Task>();
            var remainingTasks = new HashSet<Task>(tasks);

            while (remainingTasks.Count > 0)
            {
                var readyTasks = remainingTasks.Where(t => t.Dependencies.All(d => !remainingTasks.Contains(d))).ToList();

                // Sprawdza czy istnieja zadania ktore maja spelnione warunki
                if (readyTasks.Count == 0)
                {
                    // jesli nie wybierz losowe z pozostalych
                    var task = remainingTasks.ElementAt(random.Next(remainingTasks.Count));

                    task.Processor = random.Next(numberOfProcessors);
                    solution.Add(task);

                    remainingTasks.Remove(task);
                }
                else
                {
                    // wybierz losowe z zadan o spelnionych warunkach
                    var task = readyTasks[random.Next(readyTasks.Count)];

                    task.Processor = random.Next(numberOfProcessors);
                    solution.Add(task);

                    remainingTasks.Remove(task);
                }
            }

            return solution;
        }

        static bool TerminateAlgorithm(Population population, int currentGeneration, long currentEvaluation)
        {
            const int maxGenerationsWithoutImprovement = 50;
            const double minFitnessImprovementThreshold = 0.001;

            if (currentGeneration >= maxGenerationsWithoutImprovement)
            {
                var bestFitness = population.GetTop(1)[0].Fitness;
                var previousBestFitness = population.GetTop(2)[1].Fitness;

                if (bestFitness - previousBestFitness < minFitnessImprovementThreshold)
                {
                    return true; // przerwij jesli nie ma poprawy
                }
            }

            return false;
        }

        //proste wczytywanie z konsoli zadan i ich wlasciwosci oraz zapisywanie w klasie task
        static List<Task> GetTasksFromUser()
        {
            var tasks = new List<Task>();

            Console.WriteLine("Enter the number of tasks:");
            int numTasks = int.Parse(Console.ReadLine());

            for (int i = 0; i < numTasks; i++)
            {
                Console.WriteLine($"Enter details for task #{i + 1}:");
                Console.Write("Name: ");
                string name = Console.ReadLine();
                Console.Write("Duration: ");
                int duration = int.Parse(Console.ReadLine());

                Task task = new Task(name, duration);

                Console.Write("Enter the number of dependencies for this task: ");
                int numDependencies = int.Parse(Console.ReadLine());

                for (int j = 0; j < numDependencies; j++)
                {
                    Console.Write($"Enter dependency #{j + 1}: ");
                    string dependencyName = Console.ReadLine();
                    Task dependency = tasks.FirstOrDefault(t => t.Name == dependencyName);
                    if (dependency != null)
                    {
                        task.Dependencies.Add(dependency);
                    }
                    else
                    {
                        Console.WriteLine($"Task {dependencyName} does not exist.");
                        j--;
                    }
                }

                tasks.Add(task);
            }

            return tasks;
        }

        //wczytywanie ilosci procesorow
        static int GetNumberOfProcessorsFromUser()
        {
            Console.WriteLine("Enter the number of processors:");
            int numProcessors = int.Parse(Console.ReadLine());
            return numProcessors;
        }

        static int CalculateMakespan(List<List<Task>> schedule)
        {
            var taskEndTime = new Dictionary<Task, int>();

            foreach (var processor in schedule)
            {
                var currentTime = 0;

                foreach (var task in processor)
                {
                    if (!taskEndTime.ContainsKey(task))
                        taskEndTime.Add(task, 0); //dodaj do zadanie i jego czas zakonczenia do slownika

                    var dependenciesEndTime = task.Dependencies.Count > 0
                        ? task.Dependencies.Max(dependency =>
                        {
                            if (!taskEndTime.ContainsKey(dependency))
                                taskEndTime.Add(dependency, 0); //jesli w slowniku nie ma klucza rownego zaleznosci dodaj go
                            return taskEndTime[dependency];
                        })
                        : 0;

                    var taskStartTime = System.Math.Max(currentTime, dependenciesEndTime);
                    taskEndTime[task] = taskStartTime + task.Duration;

                    currentTime = taskEndTime[task];
                }
            }

            return taskEndTime.Values.Max();
        }

        static void PrintSchedule(List<List<Task>> schedule)
        {
            for (int i = 0; i < schedule.Count; i++)
            {
                Console.WriteLine($"Processor {i + 1}:");
                foreach (var task in schedule[i])
                {
                    Console.WriteLine($"{task.Name} (Duration: {task.Duration})");
                }
                Console.WriteLine();
            }
        }
    }

    
}