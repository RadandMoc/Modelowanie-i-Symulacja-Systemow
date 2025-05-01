using System.Diagnostics;
using System.Text.Json;
using System.IO;

namespace Trainer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			const int numWorkers = 4;
			string unityPath = @"UnitySim/dron.exe";  // <-- zmień na swoją ścieżkę

			for (int i = 0; i < numWorkers; i++)
			{
				string workerDir = $"Workers/Worker_{i}";
				Directory.CreateDirectory(workerDir);

				// 1. Zapisz genom jako JSON
				File.WriteAllText(Path.Combine(workerDir, "genome.json"), GenerateFakeGenome(i));

				// 2. Odpal Unity z parametrem indeksu i katalogu roboczego
				var psi = new ProcessStartInfo
				{
					FileName = unityPath,
					Arguments = $"-batchmode -nographics -executeMethod SimRunner.Run -workerId {i} -logFile log_{i}.txt",
					WorkingDirectory = workerDir,
					UseShellExecute = false
				};

				Process.Start(psi);
			}

			// 3. Oczekuj na wyniki
			for (int i = 0; i < numWorkers; i++)
			{
				string resultPath = $"Workers/Worker_{i}/result.json";
				while (!File.Exists(resultPath))
					Thread.Sleep(200);

				string fitnessJson = File.ReadAllText(resultPath);
				double fitness = JsonSerializer.Deserialize<double>(fitnessJson);
				Console.WriteLine($"Worker {i} returned fitness: {fitness}");
			}


			string GenerateFakeGenome(int index)
			{
				return JsonSerializer.Serialize(new { weights = new[] { 0.1 * index, 0.5, -0.3 } });
			}

		}
	}
}
