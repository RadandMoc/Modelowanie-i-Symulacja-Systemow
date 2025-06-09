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

public class SimRunner : MonoBehaviour
{
	private const string XmlElementNameNetwork = "Network";

	private static float simulationTimeLimit = 10f;

	[SerializeField]
	private DroneSim drone;


	[SerializeField]
	private GameObject camera;
	
	[SerializeField]
	private GameObject funcObject;

	
	private IFitnessFunction fitnessFunc;

	[SerializeField]
	private MultiCollisionBehaviour collisionBehaviour;

	private string currentDir;

	private readonly int workerId = GetArg("-workerId", 0);

    int i = 0;

	public static readonly int SEED = GetArg("-seedNo", 1234567);

	public static readonly int TURN = GetArg("-turnsNo", 2000);

    void Start()
	{
		Debug.Log($"Seed number: {SEED} - Loaded from args: {SEED != 1234567}");
		fitnessFunc = funcObject.GetComponent<TraditionalFitnessCalculate>();
		Run();
	}

	void Update()
    {
        //Debug.Log("SimRunner update");
		i++;
		if (i < TURN)
		{
			var move = drone.ClickKey();
			fitnessFunc.OnMoveMade(move, collisionBehaviour.transform);
		}
		else
		{
			Debug.Log(fitnessFunc.Evaluate());
            SaveResult(Path.Combine(currentDir, "result.json"), fitnessFunc.Evaluate());
            Application.Quit();
		}
    }

    private void SaveResult(string path, double fitness)
	{
		try
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(fitness));
			Debug.Log($"[Worker {workerId}] Wynik fitness zapisany do {path}");
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId}] B³¹d podczas zapisu wyniku do {path}. Wyj¹tek: {ex.ToString()}");
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
				Debug.LogError($"[Worker {workerId}] G³ówny element XML to nie '{XmlElementNameNetwork}' lub jest null. Znaleziono: '{(genomeNode?.Name ?? "null")}'. Plik: {path}");
				Application.Quit();
				throw new ArgumentException();
			}

			// Zgodnie z kodem Trainer.UnityCommunication.SaveGenome, parametr nodeFnIds jest 'true'
			bool expectNodeFnIds = true;
			genome = NeatGenomeXmlIO.LoadGenome(genomeNode, expectNodeFnIds);

			Debug.Log($"[Worker {workerId}] Genom ID [{genome.Id}] wczytany. Neurony: {genome.NeuronGeneList.Count}, Po³¹czenia: {genome.ConnectionGeneList.Count}. Wejœcia: {genome.InputNeuronCount}, Wyjœcia: {genome.OutputNeuronCount}");
			return genome;
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId}] Krytyczny b³¹d podczas wczytywania/parsowania genomu z {path}. Wyj¹tek: {ex.ToString()}");
			Application.Quit();
			throw new ArgumentException();
		}
	}

	private IBlackBox BlackBoxGenerator(NeatGenome genome)
	{
		IBlackBox phenome;
		try
		{
			// KROK 1: Stworzenie IActivationFunctionLibrary (musi byæ identyczna jak w Trainerze)
			// Zak³adamy, ¿e Trainer u¿ywa: DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
			IActivationFunctionLibrary activationFnLib = DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
			//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono ActivationFunctionLibrary.");

			// KROK 2: Stworzenie NeatGenomeParameters
			// Powinny byæ spójne z konfiguracj¹ Trainera. Domyœlnie FeedforwardOnly=true.
			NeatGenomeParameters neatGenomeParams = new NeatGenomeParameters();
			// Jeœli Trainer modyfikuje neatGenomeParams (np. neatGenomeParams.FeedforwardOnly = false),
			// nale¿y to odzwierciedliæ tutaj. Dla przyk³adu, za³ó¿my domyœlne wartoœci.
			// Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono NeatGenomeParameters. FeedforwardOnly: {neatGenomeParams.FeedforwardOnly}, CyclicMaxCycles: {neatGenomeParams.CyclicNetworkMaxActivationCycles}");

			// KROK 3: Stworzenie NeatGenomeFactory
			// U¿ywamy liczby wejœæ/wyjœæ z wczytanego genomu.

			NeatGenomeFactory genomeFactory = new NeatGenomeFactory(
				inputNeuronCount: genome.InputNeuronCount,
				outputNeuronCount: genome.OutputNeuronCount,
				neatGenomeParams: neatGenomeParams,
				activationFnLibrary: activationFnLib
			);

			//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono NeatGenomeFactory.");

			// KROK 4: Przypisanie GenomeFactory do wczytanego genomu
			// To kluczowy krok, aby genome.ActivationFnLibrary by³o dostêpne dla dekodera.
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
				Debug.LogError($"[Worker {workerId} | Genom {genome.Id}] KRYTYCZNY B£¥D: Fenotyp jest null po dekodowaniu. Genom mo¿e byæ nieprawid³owy lub dekoder zawiód³.");
				Application.Quit();
				throw new ArgumentException();
			}

			return phenome;

		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId} | Genom {genome.Id}] KRYTYCZNY B£¥D podczas tworzenia fenotypu (konfiguracja fabryki/dekodera). Wyj¹tek: {ex.ToString()}");
			Application.Quit();
			throw new ArgumentException();
		}
	}

	public void Run()
	{
		currentDir = Directory.GetCurrentDirectory();

		//Debug.Log($"[Worker {workerId}] Katalog roboczy: {currentDir}");
		//Debug.Log($"[Worker {workerId}] Próba wczytania genomu z: {genomePath}");

		NeatGenome genome = ReadGenome(Path.Combine(currentDir, "genome.xml"));

		// Genom wczytany przez NeatGenomeXmlIO.LoadGenome() ma _genomeFactory == null.
		// Musimy stworzyæ i przypisaæ fabrykê, aby NeatGenomeDecoder móg³ poprawnie dzia³aæ,
		// poniewa¿ dekoder poœrednio korzysta z genome.ActivationFnLibrary (która zale¿y od fabryki).

		IBlackBox phenome = BlackBoxGenerator(genome); // Zdekodowana sieæ neuronowa
		

		// Wywo³anie logiki symulacji z gotowym fenotypem
		InitializeDroneLogic(phenome, genome.Id, workerId);
		
		//Debug.Log($"[Worker {workerId}] Logika symulacji zakoñczona. Fitness dla genomu ID [{genome.Id}]: {fitness}");

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

	void InitializeDroneLogic(IBlackBox phenome, uint genomeId, int workerId)
	{
		//Debug.Log($"[Worker {workerId} | Genom {genomeId}] Wykonywanie logiki symulacji z fenotypem. Wejœcia: {phenome.InputCount}, Wyjœcia: {phenome.OutputCount}");
		var obj = GameObject.Find("Drone_red");

		DroneKinematics droneKin = obj.GetComponent<DroneKinematics>();
		
		DronePositionGenerator posGenerator = new DronePositionGenerator();
		(Vector3 vec, Quaternion rot) result = posGenerator.GeneratePositionRotation(new System.Random(SEED));
		droneKin.transform.position = result.vec;
		droneKin.transform.rotation = result.rot;
		camera.transform.position = result.vec;
		camera.transform.rotation = result.rot;
		
        IGetInputs droneKinematics = droneKin;
        IGetInputs raycastHititng = obj.GetComponent<RaycastHitting>();



		drone.Initialize(phenome, new List<IGetInputs>() { droneKinematics, raycastHititng });

	}
}
