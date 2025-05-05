using System.Diagnostics;
using System.Text.Json;
using System.IO;

namespace Trainer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			List<string> genomes = new List<string>();
			for(int i = 0; i < 10; i++)
			{
				genomes.Add(GenerateFakeGenome(i));
			}

			UnityCommunication unityComm = new UnityCommunication(6);
			ICollection<double> results = unityComm.RunSimulations(genomes);
			foreach (var result in results)
			{
				Console.WriteLine(result);
			}

			string GenerateFakeGenome(int index)
			{
				return JsonSerializer.Serialize(new { weights = new[] { 0.1 * index, 0.5, -0.3 } });
			}

		}
	}
}
