using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace Trainer
{
	internal class UnityCommunication
	{
		private int unityThreads;
		private NeatGenomeFactory genomeFactory;
		public double LastBestFitness { get; private set; } = 0;
		public static readonly string UNITY_PATH = Path.Combine(AppContext.BaseDirectory, @"..", "..", "..", "..", @"UnitySim", "Drones for MiSS.exe");
		public static readonly string WORKER_PATH = Path.Combine(AppContext.BaseDirectory, @"..", "..", "..", "..", $"Workers/Worker_");
		internal UnityCommunication(int unityThreads)
		{
			this.unityThreads = unityThreads;
		}
		internal UnityCommunication() : this(1) { }

		private void SaveGenome(NeatGenome genome, int workerId)
		{
			string workerDir = $"{WORKER_PATH}{workerId}";
			Directory.CreateDirectory(workerDir);
			//File.WriteAllText(Path.Combine(workerDir, "genome.json"), genome);
			string filePath = Path.Combine(workerDir, "genome.xml");
			try
			{
				using (XmlWriter xw = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true }))
				{
					// Zakładamy, że nodeFnIds=true jest odpowiednie dla Twojego przypadku (często wymagane)
					// Jeśli nie potrzebujesz ID funkcji aktywacji dla węzłów, zmień na false.
					NeatGenomeXmlIO.Write(xw, genome, true); // - Użycie metody Write z NeatGenomeXmlIO
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Błąd podczas zapisu genomu {genome.Id} dla workera {workerId}: {ex.Message}");
				// Rozważ logowanie błędu lub inną obsługę
			}
		}

		public void ClearWorkers()
		{
			for (int i = 0; i < unityThreads; i++)
			{
				try
				{
					File.Delete($"{WORKER_PATH}{i}/result.json");
				}
				catch (DirectoryNotFoundException) { }
			}
		}

		public void InitializeGenomeFactory(NeatGenomeFactory factory)
        {
            genomeFactory = factory;
        }

        private string? GetFitness(int workerId)
		{
			string resultPath = $"{WORKER_PATH}{workerId}/result.json";
			if (!File.Exists(resultPath))
				return null;
			while (true)
			{
				try
				{
					string fitnessJson = File.ReadAllText(resultPath);
					return fitnessJson;
				}
				catch (IOException)
				{
					Thread.Sleep(20);
				}
			}
		}

	

            public void RunSimulations(ICollection<NeatGenome> genomes)
        {
            LastBestFitness = 0;
            int checkingWorker = 0;
            Dictionary<int, uint> activeThreads = new Dictionary<int, uint>();

            // 1. Stwórz słownik, który mapuje unikalne ID genomu na listę wszystkich jego wystąpień.
            var genomesById = genomes
                .GroupBy(g => g.Id)
                .ToDictionary(group => group.Key, group => group.ToList());

            // 2. Do kolejki dodaj tylko unikalne genomy (po jednym dla każdego ID).
			

            Queue<NeatGenome> genomesToCalculateFitness = new Queue<NeatGenome>(
                genomesById.Values.Select(genomeList => genomeList[0])
            );

            int seed = new Random().Next(0, 1000000);
			int turn = new Random().Next(2000, 4000);

            // Główna pętla będzie działać dopóki kolejka lub aktywne wątki nie będą puste.
            while (genomesToCalculateFitness.Count > 0 || activeThreads.Count > 0)
            {
                if (activeThreads.ContainsKey(checkingWorker))
                {
                    string? fitnessJson = GetFitness(checkingWorker);
                    if (!Object.Equals(fitnessJson, null))
                    {
                        double result = JsonSerializer.Deserialize<double>(fitnessJson);
                        LastBestFitness = Math.Max(LastBestFitness, result);
                        File.Delete($"{WORKER_PATH}{checkingWorker}/result.json");

                        uint finishedGenomeId = activeThreads[checkingWorker];

                        // 3. Przypisz wynik fitness wszystkim genomom o tym samym ID.
                        foreach (var genome in genomesById[finishedGenomeId])
                        {
                            genome.EvaluationInfo.SetFitness(result);
                        }

                        Console.WriteLine($"Worker {checkingWorker} finished processing genome {finishedGenomeId} with fitness {result}");
                        activeThreads.Remove(checkingWorker);
                    }
                }

                if ((!activeThreads.ContainsKey(checkingWorker)) && genomesToCalculateFitness.Count > 0)
                {
                    NeatGenome nextGenome = genomesToCalculateFitness.Dequeue();

					Console.WriteLine($"Worker {checkingWorker} processing genome {nextGenome.Id}");
                    activeThreads.Add(checkingWorker, nextGenome.Id);

                    // POPRAWKA: Przekazujemy bezpośrednio obiekt 'nextGenome'.
                    SaveGenome(nextGenome, checkingWorker);

                    var psi = new ProcessStartInfo
                    {
                        FileName = UNITY_PATH,
                        Arguments = $"-executeMethod SimRunner.Run -workerId {checkingWorker} -turnsNo {turn} -seedNo {seed} -logFile log_{checkingWorker}.txt -screen-width 800 -screen-height 600 -window-mode borderless",
                        WorkingDirectory = $"{WORKER_PATH}{checkingWorker}",
                        UseShellExecute = false
                    };

                    Process.Start(psi);
                }

                checkingWorker = (checkingWorker + 1) % unityThreads;
				Thread.Sleep(25); 
			}
		}
	}
}
