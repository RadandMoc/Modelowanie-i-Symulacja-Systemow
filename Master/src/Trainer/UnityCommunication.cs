using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		#region Fields
		private int unityThreads;
		private NeatGenomeFactory genomeFactory;
		public double LastBestFitness { get; private set; } = 0;
		public static readonly string UNITY_PATH = Path.Combine(AppContext.BaseDirectory, @"..", "..", "..", "..", @"UnitySim", "Drones for MiSS.exe");
		public static readonly string WORKER_PATH = Path.Combine(AppContext.BaseDirectory, @"..", "..", "..", "..", $"Workers/Worker_");
		public static readonly int MAX_GENOMES_PER_WORKER = 10; // Maksymalna liczba genomów na jednego workera, może być dostosowana
		public static readonly bool CAMERA = true; // Czy kamera ma być aktywna w symulacji Unity
		public static readonly int NUM_REPETITIONS_SIMULATIONS = 3; // Liczba powtórzeń symulacji dla każdego genomu, może być dostosowana
		#endregion Fields

		#region Constructors
		internal UnityCommunication(int unityThreads)
		{
			this.unityThreads = unityThreads;
		}
		internal UnityCommunication() : this(1) { }
		#endregion Constructors

		#region Private Methods
		private void SaveGenomes(List<NeatGenome> genomes, int workerId)
		{
			string workerDir = $"{WORKER_PATH}{workerId}";
			Directory.CreateDirectory(workerDir);
			for (int i = 0; i < genomes.Count; i++)
			{
				NeatGenome genome = genomes[i];
				string filePath = Path.Combine(workerDir, $"genome{i}.xml");
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
		}

		private Dictionary<uint, double>? GetFitnesses(int workerId)
		{
			string resultPath = $"{WORKER_PATH}{workerId}/result.json";
			if (!File.Exists(resultPath))
				return null;
			while (true)
			{
				try
				{
					string fitnessJson = File.ReadAllText(resultPath);
					Dictionary<uint, double>? result = JsonSerializer.Deserialize<Dictionary<uint, double>>(fitnessJson);
					return result;
				}
				catch (IOException)
				{
					Thread.Sleep(20);
				}
			}
		}
		#endregion Private Methods

		#region Public Methods
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

		public void RunSimulations(ICollection<NeatGenome> genomes)
		{
			LastBestFitness = 0;
			Dictionary<uint, double> fitnessesSum = new Dictionary<uint, double>();

			int checkingWorker = 0;
			Dictionary<int, List<uint>> activeThreads = new(); // Key-no. worker, value-genome id
			HashSet<uint> genomeValidator = new HashSet<uint>();
			foreach (NeatGenome gen in genomes)
			{
				if (genomeValidator.Contains(gen.Id))
					Console.WriteLine("POWTARZA SIE ID !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!! !!!!!!!!!!!!!!!!");
				else
				{
					genomeValidator.Add(gen.Id);
					fitnessesSum.Add(gen.Id, 0); // Inicjalizuj sumę fitnessów dla każdego genomu
				}
			}
			for (int rep = 0; rep < NUM_REPETITIONS_SIMULATIONS; rep++)
			{
				Dictionary<uint, NeatGenome> genomeDict = genomes.ToDictionary(g => g.Id); //key-genome id, value-genome
				Queue<NeatGenome> genomesToCalculateFitness = new(genomes); //[.. genomes];  genomes.ToList();
				int seed = new Random().Next(0, 1000000); // Losowy seed dla symulacji, może być przekazany do Unity
				int turn = new Random().Next(2800, 4800);
				while (genomeDict.Count > 0)
				{
					if (activeThreads.ContainsKey(checkingWorker))
					{
						Dictionary<uint, double>? fitnesses = GetFitnesses(checkingWorker);
						if (!Object.Equals(fitnesses, null))
						{
							File.Delete($"{WORKER_PATH}{checkingWorker}/result.json");
							foreach (var fitness in fitnesses)
							{
								fitnessesSum[fitness.Key] += fitness.Value; // Sumuj fitnessy dla każdego genomu
								//genomeDict[fitness.Key].EvaluationInfo.SetFitness(fitness.Value); //aktualizuj fitness
								Console.WriteLine($"Worker {checkingWorker} finished processing genome {fitness.Key} with fitness {fitness.Value}");
								genomeDict.Remove(fitness.Key);
							}
							activeThreads.Remove(checkingWorker);
						}
					}

					if ((!activeThreads.ContainsKey(checkingWorker)) && genomesToCalculateFitness.Count > 0)
					{
						int genomesCount = Math.Min(MAX_GENOMES_PER_WORKER, genomesToCalculateFitness.Count);
						List<NeatGenome> genomesToSend = new();
						List<uint> genomeIdsToSend = new();
						for (int i = 0; i < genomesCount; i++)
						{
							NeatGenome nextGenome = genomesToCalculateFitness.Dequeue();
							genomesToSend.Add(nextGenome);
							genomeIdsToSend.Add(nextGenome.Id);
						}
						activeThreads.Add(checkingWorker, genomeIdsToSend);
						SaveGenomes(genomesToSend, checkingWorker);
						var psi = new ProcessStartInfo
						{
							FileName = UNITY_PATH,
							//Arguments = $"-batchmode -nographics -executeMethod SimRunner.Run -workerId {checkingWorker} -seedNo {seed} -logFile log_{checkingWorker}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}.txt -camera {Convert.ToInt32(CAMERA)} -genomesCount {genomesCount}",
							//Arguments = $"-executeMethod SimRunner.Run -workerId {checkingWorker} -turnsNo {turn} -seedNo {seed} -logFile log_{checkingWorker}.txt -screen-width 800 -screen-height 600 -window-mode borderless -camera {Convert.ToInt32(CAMERA)} -genomesCount {genomesCount}",
							Arguments = $"-executeMethod SimRunner.Run -workerId {checkingWorker} -turnsNo {turn} -seedNo {seed} -logFile log_{checkingWorker}.txt -screen-width 800 -screen-height 600 -window-mode windowed -camera {Convert.ToInt32(CAMERA)} -genomesCount {genomesCount}",
							WorkingDirectory = $"{WORKER_PATH}{checkingWorker}",
							UseShellExecute = false
						};

						Process.Start(psi);
					}
					checkingWorker = (checkingWorker + 1) % unityThreads;
					Thread.Sleep(25);
				}
			}
			foreach(var gen in genomes)
			{
				gen.EvaluationInfo.SetFitness(fitnessesSum[gen.Id]);
				Console.WriteLine($"Genom ID: {gen.Id} -> Suma fitnessów: {fitnessesSum[gen.Id]}");
				LastBestFitness = Math.Max(LastBestFitness, fitnessesSum[gen.Id]);
			}
		}
		#endregion Public Methods
	}
}
