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


			Dictionary<int, uint> activeThreads = new Dictionary<int, uint>(); // Key-no. worker, value-genome id
			HashSet<uint> numberOfGenomeWithId = new HashSet<uint>(); // Key-no. worker, value-number of genomes assigned to worker
			List<NeatGenome> genomesToProcess = new List<NeatGenome>();
			/*
			foreach (var genome in genomes)
			{
				if (!numberOfGenomeWithId.Contains(genome.Id)) { 
				genomesToProcess.Add(genome);
				numberOfGenomeWithId.Add(genome.Id); }

                else 
				{
					genomesToProcess.Add(genomeFactory.CreateGenomeCopy(genome, genomeFactory.NextGenomeId(), genome.BirthGeneration));
				}
            }
			*/
			HashSet<uint> gg = new HashSet<uint>();
			foreach (NeatGenome gen in genomes)
			{
				if (gg.Contains(gen.Id))
					Console.WriteLine("POWTARZA SIE ID !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!!");
				else
					gg.Add(gen.Id);
			}

			Dictionary<uint, NeatGenome> genomeDict = genomes.ToDictionary(g => g.Id); //key-genome id, value-genome
			Queue<NeatGenome> genomesToCalculateFitness = new(genomes); //[.. genomes];  genomes.ToList();
			int seed = new Random().Next(0, 1000000); // Losowy seed dla symulacji, może być przekazany do Unity
			int turn = new Random().Next(2800, 4800);

			while (genomeDict.Count > 0)
			{
				if (activeThreads.ContainsKey(checkingWorker))
				{
					string? fitness = GetFitness(checkingWorker);
					if (!Object.Equals(fitness, null))
					{
						double result = JsonSerializer.Deserialize<double>(fitness);
						LastBestFitness = Math.Max(LastBestFitness, result);
						File.Delete($"{WORKER_PATH}{checkingWorker}/result.json");
						genomeDict[activeThreads[checkingWorker]].EvaluationInfo.SetFitness(result); //aktualizuj fitness
						Console.WriteLine($"Worker {checkingWorker} finished processing genome {activeThreads[checkingWorker]} with fitness {result}");
						genomeDict.Remove(activeThreads[checkingWorker]);
						activeThreads.Remove(checkingWorker);
					}
				}

				if ((!activeThreads.ContainsKey(checkingWorker)) && genomesToCalculateFitness.Count > 0)
				{
					NeatGenome nextGenome = genomesToCalculateFitness.Dequeue();
					activeThreads.Add(checkingWorker, nextGenome.Id);
					SaveGenome(genomeDict[nextGenome.Id], checkingWorker);
					var psi = new ProcessStartInfo
					{
						FileName = UNITY_PATH,
						//Arguments = $"-batchmode -nographics -executeMethod SimRunner.Run -workerId {checkingWorker} -seedNo {seed} -logFile log_{checkingWorker}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}.txt",
						//Arguments = $"-executeMethod SimRunner.Run -workerId {checkingWorker} -turnsNo {turn} -seedNo {seed} -logFile log_{checkingWorker}.txt -screen-width 800 -screen-height 600 -window-mode borderless",
						Arguments = $"-executeMethod SimRunner.Run -workerId {checkingWorker} -turnsNo {turn} -seedNo {seed} -logFile log_{checkingWorker}.txt -screen-width 800 -screen-height 600 -window-mode windowed",
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
