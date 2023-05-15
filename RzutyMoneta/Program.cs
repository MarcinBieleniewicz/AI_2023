using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML.Probabilistic.Distributions;
using Range = Microsoft.ML.Probabilistic.Models.Range;

// Potrzebne do wyciągnięcia średniej z inferencji
var model = new InferenceEngine();

// Zmienne
Variable<double> successParameter = Variable.Beta(1, 1); // Rozkład beta z parametrami (1, 1)
Range flips = new Range(10); // Ilość rzutów
VariableArray<bool> coinFlipData = Variable.Array<bool>(flips); // Tablica przechowująca rzuty

// Prawdopodobieństwo sukcesu
coinFlipData[flips] = Variable.Bernoulli(successParameter).ForEach(flips);

// Obserwacje
bool[] observedData = { true, true, false, true, false, true, true, true, false, false };
coinFlipData.ObservedValue = observedData;

// Inferencja
Beta inferredSuccessParameter = model.Infer<Beta>(successParameter);
double mean = inferredSuccessParameter.GetMean();

Console.WriteLine("Inferred success parameter mean: " + mean);