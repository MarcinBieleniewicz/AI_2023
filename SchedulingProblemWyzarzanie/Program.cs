﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulingProblemWyzarzanie
{
    class Program
    {
        static void Main(string[] args)
        {
            //początkowe ustawienia przez użytkownika
            var tasks = GetTasksFromUser();
            var numProcessors = GetNumberOfProcessorsFromUser();

            //pierwotne rozwiązanie
            var initialSolution = GetInitialSolution(tasks, numProcessors);
            //wyzarzanie
            var bestSolution = SimulatedAnnealing(tasks, initialSolution, 1000, 0.95, 100);

            Console.WriteLine("Best Solution:");
            PrintSchedule(bestSolution);
            Console.WriteLine("End Time: " + CalculateEndTime(bestSolution));

            Console.ReadLine();
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

        //losowa znajduje rozwiazanie ktore pozniej jest wykorzystywane do znalezienia optymalnego rozwiazania
        static List<List<Task>> GetInitialSolution(List<Task> tasks, int numProcessors)
        {
            var random = new Random();
            var initialSolution = Enumerable.Range(0, numProcessors)
                .Select(_ => new List<Task>())
                .ToList();

            foreach (var task in tasks)
            {
                var randomProcessorIndex = random.Next(numProcessors);
                initialSolution[randomProcessorIndex].Add(task);
            }

            return initialSolution;
        }

        //algorytm wyzarzania
        static List<List<Task>> SimulatedAnnealing(List<Task> tasks, List<List<Task>> initialSolution, double initialTemperature, double coolingRate, int iterations)
        {
            var currentSolution = new List<List<Task>>(initialSolution);
            var bestSolution = new List<List<Task>>(currentSolution);

            var currentEnergy = CalculateEndTime(currentSolution);
            var bestEnergy = currentEnergy;

            var temperature = initialTemperature;

            for (int i = 0; i < iterations; i++)
            {
                //znajduje nowe rozwiazanie
                var newSolution = GetNeighborSolution(currentSolution);
                //wylicza potrzebna energie
                var newEnergy = CalculateEndTime(newSolution);
                var energyDifference = newEnergy - currentEnergy;
                //wywoluje sprawdzenie
                if (ShouldAcceptSolution(energyDifference, temperature))
                {
                    currentSolution = newSolution;
                    currentEnergy = newEnergy;

                    if (currentEnergy < bestEnergy)
                    {
                        bestSolution = new List<List<Task>>(currentSolution);
                        bestEnergy = currentEnergy;
                    }
                }

                temperature *= coolingRate;
            }

            return bestSolution;
        }

        //metoda generujaca nowe rozwiazanie, przyjmuje poprzednie rozwiazanie i znajduje do niego sasiednie rozwiazanie
        static List<List<Task>> GetNeighborSolution(List<List<Task>> solution)
        {
            var random = new Random();

            //tworzy kopie aby nie przypadkiem nie zmienic poprzedniego rozwiazania
            var newSolution = new List<List<Task>>(solution);
            var processorIndex1 = random.Next(solution.Count);
            var processorIndex2 = random.Next(solution.Count);
            //sprawdza czy istnieje mozliwosc zamiany zadan miedzy procesorami
            if (newSolution[processorIndex1].Count > 0 && newSolution[processorIndex2].Count > 0)
            {
                var taskIndex1 = random.Next(newSolution[processorIndex1].Count);
                var taskIndex2 = random.Next(newSolution[processorIndex2].Count);

                var task1 = newSolution[processorIndex1][taskIndex1];
                var task2 = newSolution[processorIndex2][taskIndex2];

                //Sprawdza czy zmiana miedzy procesorami jest niezgoda z zaleznosciami miedzy zadaniami
                var dependenciesTask1 = task1.Dependencies;
                var dependenciesTask2 = task2.Dependencies;
                var validSwap = !dependenciesTask1.Any(d => newSolution[processorIndex2].Contains(d)) &&
                                !dependenciesTask2.Any(d => newSolution[processorIndex1].Contains(d));

                if (validSwap)
                {
                    newSolution[processorIndex1][taskIndex1] = task2;
                    newSolution[processorIndex2][taskIndex2] = task1;
                }
            }

            return newSolution;
        }

        //funkcja ktora wylicza czas potrzebny rozwiazaniu
        static int CalculateEndTime(List<List<Task>> solution)
        {
            var taskEndTime = new Dictionary<Task, int>();

            foreach (var processor in solution)
            {
                var endTime = 0;

                foreach (var task in processor)
                {
                    if (!taskEndTime.ContainsKey(task))
                        taskEndTime[task] = 0;

                    var dependenciesEndTime = task.Dependencies.Count > 0 ? task.Dependencies.Max(dependency => taskEndTime[dependency]) : 0;
                    var taskStartTime = Math.Max(endTime, dependenciesEndTime);
                    var taskEndTimeValue = taskStartTime + task.Duration;

                    taskEndTime[task] = taskEndTimeValue;
                    endTime = taskEndTimeValue;
                }
            }

            return solution.SelectMany(p => p).Max(task => taskEndTime[task]);
        }

        //sprawdza czy rozwiazanie jest lepsze, jesli energyDifference jest ujemne zawsze przyjmuje nowe rozwiazanie
        //w innym wypadku liczy prawdopodobienstwo zaakceptowania, jesli losowa liczba jest mniejsza niz prawdopodobienstwo akceptacji - akceptuje rozwiazanie
        static bool ShouldAcceptSolution(double energyDifference, double temperature)
        {
            if (energyDifference < 0)
                return true;

            var acceptanceProbability = Math.Exp(-energyDifference / temperature);

            var random = new Random();
            var randomValue = random.NextDouble();

            return randomValue < acceptanceProbability;
        }

        //wypisywanie rozwiazania
        static void PrintSchedule(List<List<Task>> schedule)
        {
            for (int i = 0; i < schedule.Count; i++)
            {
                Console.WriteLine($"Processor {i + 1}: {string.Join(" -> ", schedule[i].Select(t => t.Name))}");
            }
        }
    }
}