using System.Diagnostics;
using System.Text.Json;
using System.IO;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Core;

namespace Trainer
{
	internal class Program
	{
		static NeatGenomeFactory CreateGenomeFactory(int inputNeuronCount, int outputNeuronCount)
		{
			//var activationFnLibrary = SharpNeat.Network.ActivationFunctions.DefaultActivationFunctionLibrary.CreateLibraryNeat();
			var activationFnLibrary = DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
			return new NeatGenomeFactory(inputNeuronCount, outputNeuronCount, activationFnLibrary);
		}

		static void Main(string[] args)
		{
			List<NeatGenome> genomes = new List<NeatGenome>();
			NeatGenomeFactory genomeFactory = CreateGenomeFactory(6 * 5 + 9, 10);
			// 6 promieni, każdy z 5 neuronami (dystans, czy myć, czy karany za umycie, czy ruchome, czy uderzalne) + 9 o pozycji drona

			for (uint i = 0; i < 10; i++)
			{
				genomes.Add(GenerateNeat(genomeFactory, i));
			}

			Console.WriteLine($"Utworzono {genomes.Count} genomów do symulacji.");

			int numberOfUnityThreads = 4;
			UnityCommunication unityComm = new UnityCommunication(numberOfUnityThreads);

			Console.WriteLine($"Uruchamianie symulacji na {numberOfUnityThreads} wątkach...");

			// Wywołanie RunSimulations - typ void
			unityComm.RunSimulations(genomes);

			Console.WriteLine("\nSymulacje zakończone. Odczytane wyniki fitness z genomów:");

			// Odczytaj wyniki bezpośrednio z obiektów genomów
			foreach (var genome in genomes)
			{
				// Odczytaj fitness przez właściwość Fitness, która odzwierciedla wartość ustawioną przez SetFitness()
				Console.WriteLine($"Genom ID: {genome.Id} -> Fitness: {genome.EvaluationInfo.Fitness}");
			}

			Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
			Console.ReadLine();
		}


		static NeatGenome GenerateNeat(NeatGenomeFactory factory, uint genomeId)
		{
			int inputNumOfNeurons = factory.InputNeuronCount;
			int outputNumOfNeurons = factory.OutputNeuronCount;
			int hiddenNumOfNeurons = outputNumOfNeurons;


			var neuronGeneList = new NeuronGeneList(inputNumOfNeurons + outputNumOfNeurons);
			for (uint i = 0; i < inputNumOfNeurons; i++)
			{
				neuronGeneList.Add(new NeuronGene(i, NodeType.Input, 0));
			}
			for (uint i = (uint)inputNumOfNeurons; i < inputNumOfNeurons + hiddenNumOfNeurons; i++)
			{
				neuronGeneList.Add(new NeuronGene(i, NodeType.Hidden, 0));
			}
			for (uint i = (uint)(inputNumOfNeurons + hiddenNumOfNeurons);
				i < inputNumOfNeurons + hiddenNumOfNeurons + outputNumOfNeurons; i++)
			{
				neuronGeneList.Add(new NeuronGene(i, NodeType.Output, 0));
			}

			var connectionGeneList = new ConnectionGeneList(inputNumOfNeurons * hiddenNumOfNeurons + outputNumOfNeurons);
			Random random = new Random();
			for (uint i = 0; i < inputNumOfNeurons; i++)
			{
				for (uint j = (uint)inputNumOfNeurons; j < inputNumOfNeurons + hiddenNumOfNeurons; j++)
				{
					uint connId = factory.InnovationIdGenerator.NextId;
					connectionGeneList.Add(new ConnectionGene(connId, i, j, random.NextDouble()));
				}
			}
			if (hiddenNumOfNeurons == outputNumOfNeurons)
			{
				for (uint i = (uint)inputNumOfNeurons; i < inputNumOfNeurons + hiddenNumOfNeurons; i++)
				{
					uint connId = factory.InnovationIdGenerator.NextId;
					connectionGeneList.Add(new ConnectionGene(connId, i, i + (uint)hiddenNumOfNeurons, 1));
				}
			}
			else
				throw new NotImplementedException("Gamoniom nie chciało się robić dla różnej liczby neuronów ukrytych i wyjściowych");

			var genome = new NeatGenome(factory, genomeId, 0, neuronGeneList, connectionGeneList, factory.InputNeuronCount, factory.OutputNeuronCount, true);
			// Fabryka powinna zainicjalizować EvaluationInfo. Nie musimy nic więcej robić.
			return genome;
		}

		static NeatGenome GenerateFakeNeatGenome(NeatGenomeFactory factory, uint genomeId)
		{
			uint inputNode1Id = 0;
			uint inputNode2Id = 1;
			uint outputNodeId = 2;
			uint conn1Id = factory.InnovationIdGenerator.NextId;
			uint conn2Id = factory.InnovationIdGenerator.NextId;
			uint conn3Id = factory.InnovationIdGenerator.NextId;
			uint hiddenNodeId = factory.InnovationIdGenerator.NextId;

			var neuronGeneList = new NeuronGeneList(5);
			neuronGeneList.Add(new NeuronGene(inputNode1Id, NodeType.Input, 0));
			neuronGeneList.Add(new NeuronGene(inputNode2Id, NodeType.Bias, 0));
			neuronGeneList.Add(new NeuronGene(outputNodeId, NodeType.Output, 0));
			neuronGeneList.Add(new NeuronGene(hiddenNodeId, NodeType.Hidden, 0));

			var connectionGeneList = new ConnectionGeneList(3);
			connectionGeneList.Add(new ConnectionGene(conn1Id, inputNode1Id, hiddenNodeId, (double)(genomeId + 1) * 0.1));
			connectionGeneList.Add(new ConnectionGene(conn2Id, inputNode2Id, hiddenNodeId, -0.5));
			connectionGeneList.Add(new ConnectionGene(conn3Id, hiddenNodeId, outputNodeId, 0.8));

			var genome = new NeatGenome(factory, genomeId, 0, neuronGeneList, connectionGeneList, factory.InputNeuronCount, factory.OutputNeuronCount, true);
			// Fabryka powinna zainicjalizować EvaluationInfo. Nie musimy nic więcej robić.
			return genome;
		}
		/*
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
        */
	}
}

// TODO: Czatowa wersja maina. Trzeba go przerobić zeby main działał
// TODO: Zmienić też unity na obsługę plików XML do wczytania genomu zamiast obecnego jsona.
/* 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Core; // Potrzebne dla EvaluationInfo

namespace Trainer
{
    class Program
    {
        static NeatGenomeFactory CreateGenomeFactory()
        {
            var activationFnLibrary = SharpNeat.Network.ActivationFunctions.DefaultActivationFunctionLibrary.CreateLibraryNeat();
            return new NeatGenomeFactory(inputNodeCount: 2, outputNodeCount: 1, activationFnLibrary);
        }

        static void Main(string[] args)
        {
            List<NeatGenome> genomes = new List<NeatGenome>();
            NeatGenomeFactory genomeFactory = CreateGenomeFactory();

            for (uint i = 0; i < 10; i++)
            {
                genomes.Add(GenerateFakeNeatGenome(genomeFactory, i));
            }

            Console.WriteLine($"Utworzono {genomes.Count} genomów do symulacji.");

            int numberOfUnityThreads = 4;
            UnityCommunication unityComm = new UnityCommunication(numberOfUnityThreads);

            Console.WriteLine($"Uruchamianie symulacji na {numberOfUnityThreads} wątkach...");

            // Wywołanie RunSimulations - typ void
            unityComm.RunSimulations(genomes);

            Console.WriteLine("\nSymulacje zakończone. Odczytane wyniki fitness z genomów:");

            // Odczytaj wyniki bezpośrednio z obiektów genomów
            foreach (var genome in genomes)
            {
                // Odczytaj fitness przez właściwość Fitness, która odzwierciedla wartość ustawioną przez SetFitness()
                Console.WriteLine($"Genom ID: {genome.Id} -> Fitness: {genome.EvaluationInfo.Fitness}");
            }

            Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
            Console.ReadKey();
        }

        static NeatGenome GenerateFakeNeatGenome(NeatGenomeFactory factory, uint genomeId)
        {
             uint inputNode1Id = 0;
             uint inputNode2Id = 1;
             uint outputNodeId = 2;
             uint hiddenNodeId = factory.InnovationIdGenerator.NextId;
             uint conn1Id = factory.InnovationIdGenerator.NextId;
             uint conn2Id = factory.InnovationIdGenerator.NextId;
             uint conn3Id = factory.InnovationIdGenerator.NextId;

            var neuronGeneList = new NeuronGeneList(5);
            neuronGeneList.Add(new NeuronGene(inputNode1Id, NodeType.Input, 0));
            neuronGeneList.Add(new NeuronGene(inputNode2Id, NodeType.Input, 0));
            neuronGeneList.Add(new NeuronGene(outputNodeId, NodeType.Output, factory.OutputActivationFunctionId));
             neuronGeneList.Add(new NeuronGene(hiddenNodeId, NodeType.Hidden, factory.DefaultActivationFunctionId));

            var connectionGeneList = new ConnectionGeneList(3);
            connectionGeneList.Add(new ConnectionGene(conn1Id, inputNode1Id, hiddenNodeId, (double)(genomeId + 1) * 0.1));
            connectionGeneList.Add(new ConnectionGene(conn2Id, inputNode2Id, hiddenNodeId, -0.5));
             connectionGeneList.Add(new ConnectionGene(conn3Id, hiddenNodeId, outputNodeId, 0.8));

             var genome = new NeatGenome(factory, genomeId, 0, neuronGeneList, connectionGeneList, factory.InputNeuronCount, factory.OutputNeuronCount, true);
             // Fabryka powinna zainicjalizować EvaluationInfo. Nie musimy nic więcej robić.
             return genome;
        }
    }
}
*/
