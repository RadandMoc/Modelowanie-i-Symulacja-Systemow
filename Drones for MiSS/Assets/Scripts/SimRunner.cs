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

public class SimRunner: MonoBehaviour
{
	private const string XmlElementNameNetwork = "Network";

    [SerializeField]

    private DroneSim drone;

	[SerializeField]
    private static float simulationTimeLimit = 10f;

    [SerializeField]
    private GameObject droneObject;

    void Start()
	{
		//Debug.Log("SimRunner started");
		Run();
	}

	public void Run()
	{
		int workerId = GetArg("-workerId", 0);
		string currentDir = Directory.GetCurrentDirectory();
		string genomePath = Path.Combine(currentDir, "genome.xml");
		string resultPath = Path.Combine(currentDir, "result.json");

		//Debug.Log($"[Worker {workerId}] Katalog roboczy: {currentDir}");
		//Debug.Log($"[Worker {workerId}] Próba wczytania genomu z: {genomePath}");

		NeatGenome genome;
		try
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(genomePath);

			XmlNode genomeNode = xmlDoc.DocumentElement;

			if (genomeNode == null || genomeNode.Name != XmlElementNameNetwork)
			{
				Debug.LogError($"[Worker {workerId}] G³ówny element XML to nie '{XmlElementNameNetwork}' lub jest null. Znaleziono: '{(genomeNode?.Name ?? "null")}'. Plik: {genomePath}");
				Application.Quit();
				return;
			}

			// Zgodnie z kodem Trainer.UnityCommunication.SaveGenome, parametr nodeFnIds jest 'true'
			bool expectNodeFnIds = true;
			genome = NeatGenomeXmlIO.LoadGenome(genomeNode, expectNodeFnIds);
			
			Debug.Log($"[Worker {workerId}] Genom ID [{genome.Id}] wczytany. Neurony: {genome.NeuronGeneList.Count}, Po³¹czenia: {genome.ConnectionGeneList.Count}. Wejœcia: {genome.InputNeuronCount}, Wyjœcia: {genome.OutputNeuronCount}");
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId}] Krytyczny b³¹d podczas wczytywania/parsowania genomu z {genomePath}. Wyj¹tek: {ex.ToString()}");
			Application.Quit();
			return;
		}

		// Genom wczytany przez NeatGenomeXmlIO.LoadGenome() ma _genomeFactory == null.
		// Musimy stworzyæ i przypisaæ fabrykê, aby NeatGenomeDecoder móg³ poprawnie dzia³aæ,
		// poniewa¿ dekoder poœrednio korzysta z genome.ActivationFnLibrary (która zale¿y od fabryki).

		IBlackBox phenome; // Zdekodowana sieæ neuronowa
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
			//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Przypisano GenomeFactory do wczytanego genomu.");

			// KROK 5: Stworzenie NetworkActivationScheme
			NetworkActivationScheme activationScheme;
			activationScheme = NetworkActivationScheme.CreateAcyclicScheme();
			
			/*if (genomeFactory.NeatGenomeParameters.FeedforwardOnly)
			{
				activationScheme = NetworkActivationScheme.CreateAcyclicScheme();
				//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] U¿yto Acyclic NetworkActivationScheme.");
			}
			else
			{
				Debug.LogError($"Cykliczne sieci not implemented kur³a");
				Application.Quit();
				return;
				// Dla sieci cyklicznych, u¿yj liczby cykli z parametrów genomu.
				//int cyclicNetworkDepth = genomeFactory.NeatGenomeParameters.CyclicNetworkMaxActivationCycles;
				//activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(cyclicNetworkDepth);
				//Debug.Log($"[Worker {workerId} | Genom {genome.Id}] U¿yto CyclicFixedTimestepsScheme z g³êbokoœci¹: {cyclicNetworkDepth}.");
			}*/

			// KROK 6: Stworzenie NeatGenomeDecoder
			NeatGenomeDecoder decoder = new NeatGenomeDecoder(activationScheme);

            //Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Stworzono NeatGenomeDecoder.");

            // KROK 7: Zdekodowanie genomu do fenotypu (IBlackBox)
            phenome = decoder.Decode(genome);
            Debug.Log("X 7");


            //Debug.Log($"[Worker {workerId} | Genom {genome.Id}] Genom zdekodowany do fenotypu.");

            if (phenome == null)
			{
				Debug.LogError($"[Worker {workerId} | Genom {genome.Id}] KRYTYCZNY B£¥D: Fenotyp jest null po dekodowaniu. Genom mo¿e byæ nieprawid³owy lub dekoder zawiód³.");
				Application.Quit();
				return;
			}



		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId} | Genom {genome.Id}] KRYTYCZNY B£¥D podczas tworzenia fenotypu (konfiguracja fabryki/dekodera). Wyj¹tek: {ex.ToString()}");
			Application.Quit();
			return;
		}

		// Wywo³anie logiki symulacji z gotowym fenotypem
		double fitness = RunSimulationLogic(phenome, genome.Id, workerId);
		//Debug.Log($"[Worker {workerId}] Logika symulacji zakoñczona. Fitness dla genomu ID [{genome.Id}]: {fitness}");

		try
		{
			File.WriteAllText(resultPath, JsonConvert.SerializeObject(fitness));
			//Debug.Log($"[Worker {workerId}] Wynik fitness zapisany do {resultPath}");
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId}] B³¹d podczas zapisu wyniku do {resultPath}. Wyj¹tek: {ex.ToString()}");
		}
		finally
		{
			//Debug.Log($"[Worker {workerId}] Zamykanie aplikacji.");
			Application.Quit();
		}
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

	double RunSimulationLogic(IBlackBox phenome, uint genomeId, int workerId)
	{
		//Debug.Log($"[Worker {workerId} | Genom {genomeId}] Wykonywanie logiki symulacji z fenotypem. Wejœcia: {phenome.InputCount}, Wyjœcia: {phenome.OutputCount}");
		var obj = GameObject.Find("Drone_red");
		
		IGetInputs droneKinematics = obj.GetComponent<DroneKinematics>();
        IGetInputs raycastHititng = obj.GetComponent<RaycastHitting>();
		Debug.Log(drone is null);

        drone.Initialize(phenome, new List<IGetInputs>() { droneKinematics, raycastHititng });

        try
		{
            // Zawsze resetuj stan przed now¹ ewaluacj¹, szczególnie dla sieci z pêtlami.
            //phenome.ResetState();

            // TODO: Zaimplementuj tutaj w³aœciw¹ logikê symulacji.
            // Poni¿ej znajduje siê tylko przyk³ad i placeholder.

            // 1. Przygotuj i ustaw sygna³y wejœciowe dla sieci
            //    Upewnij siê, ¿e liczba sygna³ów odpowiada phenome.InputCount.
            //    Przyk³ad dla dwóch wejœæ:
            //    if (phenome.InputCount >= 2)
            //    {
            //        phenome.InputSignalArray[0] = jakaœ_wartoœæ_wejœciowa_1_z_symulacji;
            //        phenome.InputSignalArray[1] = jakaœ_wartoœæ_wejœciowa_2_z_symulacji;
            //    }
            //    else if (phenome.InputCount == 1)
            //    {
            //        phenome.InputSignalArray[0] = jakaœ_wartoœæ_wejœciowa_1_z_symulacji;
            //    }
            //    else
            //    {
            //        //Debug.LogWarning($"[Worker {workerId} | Genom {genomeId}] Sieæ nie ma wejœæ (InputCount = 0).");
            //    }


            // 2. Aktywuj sieæ
            //    Dla sieci "feedforward" (acyklicznych) zazwyczaj wystarczy jedna aktywacja.
            //    Dla sieci cyklicznych, mo¿esz potrzebowaæ wielokrotnych aktywacji w pêtli,
            //    zgodnie ze schematem aktywacji u¿ytym w dekoderze.
            //    Jeœli u¿yto NetworkActivationScheme.CreateAcyclicScheme(), jedna aktywacja jest typowa.
            // phenome.Activate();


            // 3. Odczytaj sygna³y wyjœciowe z sieci
            //    Upewnij siê, ¿e odczytujesz odpowiedni¹ liczbê wyjœæ (phenome.OutputCount).
            //    Przyk³ad dla jednego wyjœcia:
            //    double outputValue = 0.0;
            //    if (phenome.OutputCount >= 1)
            //    {
            //        outputValue = phenome.OutputSignalArray[0];
            //    }
            //    else
            //    {
            //        //Debug.LogWarning($"[Worker {workerId} | Genom {genomeId}] Sieæ nie ma wyjœæ (OutputCount = 0).");
            //    }

            // 4. Oblicz fitness na podstawie wyników symulacji (np. outputValue, zachowanie agenta)

            float simulationStartTime = Time.time;
            // Pozwól symulacji dzia³aæ przez okreœlony czas lub do zakoñczenia przez drona
            while (Time.time - simulationStartTime < simulationTimeLimit)
            {
                drone.ClickKey(); // Wywo³aj metodê symuluj¹c¹ klawisz
                                            // Opcjonalnie: SprawdŸ, czy dron zakoñczy³ zadanie (np. dotar³ do celu, rozbi³ siê)
                /*
                if (activeDroneController.IsFinished())
                {
                    Debug.Log($"Dron {currentGenomeIndex} zakoñczy³ przed czasem.");
                    break;
                }
                */
				 // Poczekaj na nastêpn¹ klatkê
            }

			Console.WriteLine($"[Worker {workerId} | Genom {genomeId}] Symulacja zakoñczona. Czas: {Time.time - simulationStartTime} sekund.");
            double calculatedFitness = UnityEngine.Random.Range(0f, 100f); // Placeholder
																		   //Debug.Log($"[Worker {workerId} | Genom {genomeId}] Placeholder symulacji wykonany. Obliczony fitness: {calculatedFitness}.");

			// SprawdŸ, czy stan sieci jest nadal wa¿ny (szczególnie po aktywacji sieci cyklicznych)
			if (!phenome.IsStateValid)
			{
				//Debug.LogWarning($"[Worker {workerId} | Genom {genomeId}] Stan fenotypu sta³ siê nieprawid³owy po aktywacji.");
				// Mo¿esz chcieæ na³o¿yæ karê na fitness w takim przypadku.
				// calculatedFitness *= 0.1; // Przyk³ad kary
			}

			return calculatedFitness;
		}
		catch (Exception ex)
		{
			Debug.LogError($"[Worker {workerId} | Genom {genomeId}] B³¹d podczas wykonywania symulacji z fenotypem. Wyj¹tek: {ex.ToString()}");
			return 0.0; // Zwróæ niski (lub ujemny) fitness w przypadku b³êdu
		}
	}
}
