using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public class SimRunner
{
	public static void Run()
	{
		int workerId = GetArg("-workerId", 0);
		string genomePath = Path.Combine(Directory.GetCurrentDirectory(), "genome.json");
		string resultPath = Path.Combine(Directory.GetCurrentDirectory(), "result.json");

		string genomeJson = File.ReadAllText(genomePath);
		var genome = JsonConvert.DeserializeObject<GenomeStub>(genomeJson);

		double fitness = RunSimulation(genome);

		File.WriteAllText(resultPath, JsonConvert.SerializeObject(fitness));
		Application.Quit();
	}

	static int GetArg(string key, int defaultValue)
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length - 1; i++)
			if (args[i] == key) return int.Parse(args[i + 1]);
		return defaultValue;
	}

	static double RunSimulation(GenomeStub genome)
	{
		// TODO: symulacja, zwraca fitness na podstawie genome
		return UnityEngine.Random.Range(0f, 100f);
	}

	class GenomeStub
	{
		public float[] weights { get; set; }
	}
}
