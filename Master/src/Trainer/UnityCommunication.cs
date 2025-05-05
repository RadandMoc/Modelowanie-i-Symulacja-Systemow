using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Trainer
{
	internal class UnityCommunication
	{
		private int unityThreads;
		private readonly Dictionary<int, int> activeThreads = new Dictionary<int, int>(); // Key-no. worker, value-genome id
		public static readonly string UNITY_PATH = Path.Combine(AppContext.BaseDirectory, @"..", "..", "..", "..", @"UnitySim", "Drones for MiSS.exe");
		public static readonly string WORKER_PATH = Path.Combine(AppContext.BaseDirectory, @"..", "..", "..", "..", $"Workers/Worker_");
		internal UnityCommunication(int unityThreads)
		{
			this.unityThreads = unityThreads;
		}
		internal UnityCommunication() : this(1) { }

		private void SaveGenome(string genome, int workerId)
		{
			string workerDir = $"{WORKER_PATH}{workerId.ToString()}";
			Directory.CreateDirectory(workerDir);
			File.WriteAllText(Path.Combine(workerDir, "genome.json"), genome);
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

		public ICollection<double> RunSimulations(ICollection<string/*Jakiego typu byłaby otrzymana kolekcja genomów?*/> genomes)
		{
			Dictionary<int, string> genomeDict = new Dictionary<int, string>(); //key-genome id, value-genome
			SortedDictionary<int, string> results = new SortedDictionary<int, string>(); // key-genome id, value-fitness
			foreach (var genome in genomes)
			{
				int workerId = genomeDict.Count;
				genomeDict.Add(workerId, genome);
			}
			int checkingWorker = 0;
			int nextGenomeId = 0;
			while (genomeDict.Count > 0)
			{
				if (activeThreads.ContainsKey(checkingWorker))
				{
					string? fitness = GetFitness(checkingWorker);
					if (!Object.Equals(fitness, null))
					{
						results.Add(activeThreads[checkingWorker], fitness);
						File.Delete($"{WORKER_PATH}{checkingWorker}/result.json");
						genomeDict.Remove(activeThreads[checkingWorker]);
						activeThreads.Remove(checkingWorker);
					}
				}

				if ((!activeThreads.ContainsKey(checkingWorker)) && nextGenomeId < genomes.Count)
				{
					activeThreads.Add(checkingWorker, nextGenomeId);
					SaveGenome(genomeDict[nextGenomeId], checkingWorker);
					var psi = new ProcessStartInfo
					{
						FileName = UNITY_PATH,
						Arguments = $"-batchmode -nographics -executeMethod SimRunner.Run -workerId {checkingWorker}",// -logFile log_{checkingWorker}.txt",
						WorkingDirectory = $"{WORKER_PATH}{checkingWorker}",
						UseShellExecute = false
					};

					Process.Start(psi);
					nextGenomeId++;
				}
				checkingWorker = (checkingWorker + 1) % unityThreads;
			}

			List<double> returner = new List<double>();
			foreach (var result in results)
				returner.Add(JsonSerializer.Deserialize<double>(result.Value));

			return returner;
		}
	}
}
