using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using SharpNeat.Genomes.Neat;
using System.Xml;
using SharpNeat.Network;
using SharpNeat.Decoders.Neat;
using SharpNeat.Decoders;
using SharpNeat.Phenomes;
using Assets.Scripts;
using System.Collections.Generic;
using Unity.VisualScripting;
using SharpNeat.Core;
using System.Linq;

public class SimRunner : MonoBehaviour
{
	private const string XmlElementNameNetwork = "Network";

	private static float simulationTimeLimit = 10f;




	[SerializeField]
	private GameObject camera;



	[SerializeField]
	private List<Simulation> simulations = new List<Simulation>();

    private string currentDir;

	private readonly int workerId = GetArg("-workerId", 0);

	int i = 0;

	public static readonly int SEED = GetArg("-seedNo", 1234567);

	public static readonly int TURN = GetArg("-turnsNo", 2000);

	public static readonly bool CAMERA = GetArg("-camera", 1) == 1 ? true : false;

	public static readonly int GENOMES_COUNT = GetArg("-genomesCount", 4);

	private const int ZAXISNormalize = 1500; 

	private bool isEnd = false;

    void Start()
	{
		Debug.Log($"Seed number: {SEED} - Loaded from args: {SEED != 1234567}");
		Run();
	}

	void Update()
	{
		//Debug.Log("SimRunner update");
		i++;
		if (i < TURN && simulations.Any(x => !x.isFinished))
		{
			foreach (var sim in simulations.Where(x => !x.isFinished))
			{
				sim.TriggerMove();
            }
		}
		else
		{
			if (isEnd) 
				return;
            Debug.Log("Koniec");
			// TODO : Save results to file - for now deleted, couse it was for only one genome
			//SaveResult(Path.Combine(currentDir, "result.json"), fitnessFunc.Evaluate());
			Dictionary<uint, double> fitnesses = new Dictionary<uint, double>();
			foreach (var sim in simulations)
				fitnesses.Add(sim.GenomeId, sim.ComputeFitness());
			SaveResult(Path.Combine(currentDir, "result.json"), fitnesses);
			isEnd = true;
            Application.Quit();
		}
	}

	/// <summary>
	/// Save JSON file with fitness results.
	/// </summary>
	/// <param name="path">path were save file</param>
	/// <param name="fitnesses">dict of fitnesses, where <b>key</b> is original id of genome and <b>value</b> is a double value to save for genome</param>
	private void SaveResult(string path, Dictionary<uint, double> fitnesses)
	{
		try
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(fitnesses));
			Debug.Log($"[Worker {workerId}] Wynik fitness zapisany do {path}");
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId}] B��d podczas zapisu wyniku do {path}. Wyj�tek: {ex.ToString()}");
		}
	}

	private NeatGenome ReadGenome(string path)
	{
		NeatGenome genome;
		try
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(path);

			XmlNode genomeNode = xmlDoc.DocumentElement;

			if (genomeNode == null || genomeNode.Name != XmlElementNameNetwork)
			{
				Debug.LogError($"[Worker {workerId}] G��wny element XML to nie '{XmlElementNameNetwork}' lub jest null. Znaleziono: '{(genomeNode?.Name ?? "null")}'. Plik: {path}");
				Application.Quit();
				throw new ArgumentException();
			}

			// Zgodnie z kodem Trainer.UnityCommunication.SaveGenome, parametr nodeFnIds jest 'true'
			bool expectNodeFnIds = true;
			genome = NeatGenomeXmlIO.LoadGenome(genomeNode, expectNodeFnIds);

			Debug.Log($"[Worker {workerId}] Genom ID [{genome.Id}] wczytany. Neurony: {genome.NeuronGeneList.Count}, Po��czenia: {genome.ConnectionGeneList.Count}. Wej�cia: {genome.InputNeuronCount}, Wyj�cia: {genome.OutputNeuronCount}");
			return genome;
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId}] Krytyczny b��d podczas wczytywania/parsowania genomu z {path}. Wyj�tek: {ex.ToString()}");
			Application.Quit();
			throw new ArgumentException();
		}
	}

	private IBlackBox BlackBoxGenerator(NeatGenome genome)
	{
		IBlackBox phenome;
		try
		{
			// KROK 1: Stworzenie IActivationFunctionLibrary (musi by� identyczna jak w Trainerze)
			// Zak�adamy, �e Trainer u�ywa: DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
			IActivationFunctionLibrary activationFnLib = DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
			//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono ActivationFunctionLibrary.");

			// KROK 2: Stworzenie NeatGenomeParameters
			// Powinny by� sp�jne z konfiguracj� Trainera. Domy�lnie FeedforwardOnly=true.
			NeatGenomeParameters neatGenomeParams = new NeatGenomeParameters();
			// Je�li Trainer modyfikuje neatGenomeParams (np. neatGenomeParams.FeedforwardOnly = false),
			// nale�y to odzwierciedli� tutaj. Dla przyk�adu, za��my domy�lne warto�ci.
			// Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono NeatGenomeParameters. FeedforwardOnly: {neatGenomeParams.FeedforwardOnly}, CyclicMaxCycles: {neatGenomeParams.CyclicNetworkMaxActivationCycles}");

			// KROK 3: Stworzenie NeatGenomeFactory
			// U�ywamy liczby wej��/wyj�� z wczytanego genomu.

			NeatGenomeFactory genomeFactory = new NeatGenomeFactory(
				inputNeuronCount: genome.InputNeuronCount,
				outputNeuronCount: genome.OutputNeuronCount,
				neatGenomeParams: neatGenomeParams,
				activationFnLibrary: activationFnLib
			);

			//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono NeatGenomeFactory.");

			// KROK 4: Przypisanie GenomeFactory do wczytanego genomu
			// To kluczowy krok, aby genome.ActivationFnLibrary by�o dost�pne dla dekodera.
			genome.GenomeFactory = genomeFactory;

			if (!CyclicNetworkTest.IsNetworkCyclic(genome))
				phenome = FastAcyclicNetworkFactory.CreateFastAcyclicNetwork(genome, false);
			else
			{
				NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(40);
				phenome = CyclicNetworkFactory.CreateCyclicNetwork(genome, activationScheme, false);
			}

			//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Genom zdekodowany do fenotypu.");

			if (phenome == null)
			{
				Debug.LogError($"[Worker {workerId} | Genom {genome.Id}] KRYTYCZNY B��D: Fenotyp jest null po dekodowaniu. Genom mo�e by� nieprawid�owy lub dekoder zawi�d�.");
				Application.Quit();
				throw new ArgumentException();
			}

			return phenome;

		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId} | Genom {genome.Id}] KRYTYCZNY B��D podczas tworzenia fenotypu (konfiguracja fabryki/dekodera). Wyj�tek: {ex.ToString()}");
			Application.Quit();
			throw new ArgumentException();
		}
	}

	public void Run()
	{
		currentDir = Directory.GetCurrentDirectory();

        //Debug.Log($"[Worker {workerId}] Katalog roboczy: {currentDir}");
        //Debug.Log($"[Worker {workerId}] Pr�ba wczytania genomu z: {genomePath}");
        DronePositionGenerator posGenerator = new DronePositionGenerator();
        (Vector3 vec, Quaternion rot) result = posGenerator.GeneratePositionRotation(new System.Random(SEED));
		List<Simulation> newSimulations = new List<Simulation>();
        for (int i = 0; i < GENOMES_COUNT; i++)
		{
			NeatGenome genome = ReadGenome(Path.Combine(currentDir, $"genome{i}.xml"));

			// Genom wczytany przez NeatGenomeXmlIO.LoadGenome() ma _genomeFactory == null.
			// Musimy stworzy� i przypisa� fabryk�, aby NeatGenomeDecoder m�g� poprawnie dzia�a�,
			// poniewa� dekoder po�rednio korzysta z genome.ActivationFnLibrary (kt�ra zale�y od fabryki).

			IBlackBox phenome = BlackBoxGenerator(genome); // Zdekodowana sie� neuronowa
            Vector3 vec = new Vector3(result.vec.x, result.vec.y, result.vec.z + i * ZAXISNormalize);

			// Wywo�anie logiki symulacji z gotowym fenotypem
			//InitializeDroneLogic(phenome, genome.Id, workerId, vec, result.rot);
			simulations[i].gameObject.SetActive(true);
			simulations[i].InitializeDroneLogic(phenome, genome.Id, workerId, vec, result.rot, i  * ZAXISNormalize);
            newSimulations.Add(simulations[i]);

        }
		simulations = newSimulations;

        //Debug.Log($"[Worker {workerId}] Logika symulacji zako�czona. Fitness dla genomu ID [{genome.Id}]: {fitness}");

        //Debug.Log($"[Worker {workerId}] Zamykanie aplikacji.");
        //Application.Quit();

    }

	static int GetArg(string key, int defaultValue)
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (args[i] == key)
			{
				if (int.TryParse(args[i + 1], out int val))
				{
					return val;
				}
				//Debug.LogWarning($"[ArgParser] Argument for '{key}': Value '{args[i + 1]}' is not a valid integer. Using default: {defaultValue}.");
			}
		}
		return defaultValue;
	}

	/*
	void InitializeDroneLogic(IBlackBox phenome, uint genomeId, int workerId, Vector3 vec, Quaternion rot)
	{
		//Debug.Log($"[Worker {workerId} | Genom {genomeId}] Wykonywanie logiki symulacji z fenotypem. Wej�cia: {phenome.InputCount}, Wyj�cia: {phenome.OutputCount}");
		var obj = GameObject.Find("Drone_red");



		DroneKinematics droneKin = obj.GetComponent<DroneKinematics>();

		
		

		//camera.transform.position = result.vec;
		//camera.transform.rotation = result.rot;

		IGetInputs droneKinematics = droneKin;
		IGetInputs raycastHititng = obj.GetComponent<RaycastHitting>();



		drone.Initialize(phenome, new List<IGetInputs>() { droneKinematics, raycastHititng });

	}*/
}
