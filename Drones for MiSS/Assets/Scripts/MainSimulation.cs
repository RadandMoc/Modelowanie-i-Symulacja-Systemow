using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Potrzebne dla Coroutines
using System.Collections.Generic;
using Assets.Scripts; // Potrzebne dla List (jeśli NEAT tego wymaga)
// using SharpNeat; // Przykładowa przestrzeń nazw dla biblioteki NEAT - dostosuj do używanej biblioteki!

public class MainSimulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int totalGenerations = 100; // Ile generacji ma trwać trening
    public int populationSize = 50;   // Rozmiar populacji w każdej generacji (często definiowany przez NEAT)
    public float simulationTimeLimit = 600f; // Maksymalny czas trwania symulacji dla jednego drona (w sekundach)

    [Header("Scene Management")]
    public string simulationSceneName = "DroneSimulationScene"; // Nazwa sceny do przeładowania

    [Header("Prefabs and Controllers")]
    public GameObject dronePrefab; // Prefab drona, który będzie instancjonowany

    // --- Zmienne stanu NEAT ---
    // Zakładamy, że masz jakąś klasę lub strukturę zarządzającą algorytmem NEAT.
    // To jest tylko przykład - dostosuj do swojej implementacji NEAT!
    // Przykład: użycie popularnej biblioteki SharpNEAT
    // private NeatEvolutionAlgorithm<NeatGenome> neatAlgorithm;
    // private List<NeatGenome> currentPopulation; // Lista genomów w bieżącej generacji

    // --- Zmienne stanu symulacji ---
    private static int currentGeneration = 0; // Używamy static, aby przetrwać przeładowanie sceny
    private static int currentGenomeIndex = 0; // Indeks aktualnie testowanego genomu/drona
    private static bool isSimulationRunning = false; // Flaga zapobiegająca wielokrotnemu uruchomieniu


    [SerializeField]
    private DroneSim3 droneController ; // Referencja do kontrolera aktywnego drona

    private IFitnessFunction fitnessFunction;

    // --- Singleton Pattern (Opcjonalny, ale pomocny) ---
    // Aby upewnić się, że istnieje tylko jedna instancja MainSimulation
    // i że przetrwa ona przeładowania sceny.
    private static MainSimulation instance;

    void Awake()
    {
        // Prosty Singleton Pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Kluczowe: obiekt nie zostanie zniszczony przy ładowaniu sceny
        }
        else if (instance != this)
        {
            Debug.LogWarning("Znaleziono drugą instancję MainSimulation. Niszczenie nowej.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Uruchom cykl symulacji tylko jeśli nie jest już uruchomiony
        // i jeśli jesteśmy w odpowiedniej scenie (lub przy pierwszym uruchomieniu)
        //Time.timeScale = 0.5f;
        Debug.Log($"Prefab drona: {dronePrefab}");
        if (!isSimulationRunning && SceneManager.GetActiveScene().name == simulationSceneName)
        {
            Debug.Log($"Rozpoczynanie symulacji od Generacji: {currentGeneration}, Genomu: {currentGenomeIndex}");
            StartCoroutine(RunSimulationCycle());
        }
        else if (SceneManager.GetActiveScene().name != simulationSceneName)
        {
            Debug.Log($"Ładowanie sceny symulacji: {simulationSceneName}");
            SceneManager.LoadScene(simulationSceneName);
            // Coroutine RunSimulationCycle zostanie wywołana w Start() po załadowaniu nowej sceny
        }
    }

    IEnumerator RunSimulationCycle()
    {
        isSimulationRunning = true;
        Debug.Log("--- Rozpoczęcie cyklu symulacji ---");

        // --- Inicjalizacja NEAT (jeśli to pierwsza generacja) ---
        if (currentGeneration == 0 && currentGenomeIndex == 0)
        {
            Debug.Log("Inicjalizacja algorytmu NEAT i pierwszej populacji.");
            InitializeNeat(); // Implementacja tej metody zależy od używanej biblioteki NEAT
            // Ustaw populationSize na podstawie faktycznego rozmiaru populacji z NEAT
            // populationSize = neatAlgorithm.Population.Count;
        }

        // --- Główna pętla przez generacje ---
        while (currentGeneration < totalGenerations)
        {
            Debug.Log($"=== Generacja {currentGeneration} ===");

            // --- Pętla przez jednostki (genomy) w populacji ---
            while (currentGenomeIndex < populationSize)
            {
                Debug.Log($"-- Testowanie Genomu {currentGenomeIndex} --");

                // 1. Przygotowanie środowiska (jest resetowane przez przeładowanie sceny)
                // W tym miejscu możesz dodać kod resetujący specyficzne elementy,
                // jeśli nie chcesz przeładowywać całej sceny za każdym razem.

                // 2. Instancjonowanie drona

                GameObject droneInstance = Instantiate(dronePrefab, GetStartPosition(), Quaternion.identity); // Użyj odpowiedniej pozycji startowej
                /*
                activeDroneController = droneInstance.GetComponent<DroneController>();

                if (activeDroneController == null)
                {
                    Debug.LogError("Prefab drona nie ma komponentu DroneController!");
                    yield break; // Zakończ coroutine w razie błędu
                }
                */

                // 3. Przypisanie sieci neuronowej (genomu) do drona
                // To zależy od implementacji NEAT i DroneController
                // Przykład:
                // var currentGenome = neatAlgorithm.Population[currentGenomeIndex];
                // activeDroneController.AssignNetwork(currentGenome); // Musisz zaimplementować tę metodę w DroneController

                // 4. Uruchomienie symulacji dla jednego drona
                float simulationStartTime = Time.time;
                // Pozwól symulacji działać przez określony czas lub do zakończenia przez drona
                while (Time.time - simulationStartTime < simulationTimeLimit)
                {
                    droneController.ClickKey(); // Wywołaj metodę symulującą klawisz
                    // Opcjonalnie: Sprawdź, czy dron zakończył zadanie (np. dotarł do celu, rozbił się)
                    /*
                    if (activeDroneController.IsFinished())
                    {
                        Debug.Log($"Dron {currentGenomeIndex} zakończył przed czasem.");
                        break;
                    }
                    */
                    yield return null; // Poczekaj na następną klatkę
                }

                // 5. Ocena fitnessu drona
                //float fitness = activeDroneController.CalculateFitness(); // Metoda w DroneController obliczająca wynik
                //Debug.Log($"Genom {currentGenomeIndex} uzyskał Fitness: {fitness}");

                // 6. Zapisanie fitnessu do genomu NEAT
                // Przykład:
                // neatAlgorithm.Population[currentGenomeIndex].Fitness = fitness;

                // 7. Sprzątanie - zniszczenie instancji drona
                Destroy(droneInstance);
                //activeDroneController = null;

                // 8. Przejście do następnego genomu
                currentGenomeIndex++;

                // 9. Przeładowanie sceny, aby zresetować środowisko dla następnego drona
                // Upewnij się, że zmienne static (currentGeneration, currentGenomeIndex) przetrwają!
                Debug.Log($"Przeładowywanie sceny '{simulationSceneName}' dla następnego drona...");
                yield return SceneManager.LoadSceneAsync(simulationSceneName); // Asynchroniczne ładowanie jest lepsze

                // Po przeładowaniu sceny, Start() zostanie wywołane ponownie,
                // a ponieważ isSimulationRunning = true, nie uruchomi nowego cyklu,
                // ale ta coroutine będzie kontynuowana dzięki 'static'.
                // Musimy jednak poczekać jedną klatkę, aby Start() i Awake() w nowej scenie zdążyły się wykonać.
                var obj = GameObject.Find("Drone"); // Upewnij się, że prefab jest w zasobach
                dronePrefab = obj;
                droneController = obj.GetComponent<DroneSim3>();
                yield return null;

            } // Koniec pętli przez genomy

            // --- Ewolucja następnej generacji (po przetestowaniu całej populacji) ---
            Debug.Log($"Zakończono ocenę generacji {currentGeneration}. Ewoluowanie następnej generacji...");
            EvolveNextGeneration(); // Implementacja tej metody zależy od biblioteki NEAT

            currentGeneration++;
            currentGenomeIndex = 0; // Resetuj indeks genomu dla nowej generacji

            // Opcjonalnie: Zapisz postęp, najlepszy genom itp.

        } // Koniec pętli przez generacje

        Debug.Log("=== Zakończono wszystkie generacje ===");
        isSimulationRunning = false;
        // Opcjonalnie: Zapisz finalny najlepszy genom/sieć
    }

    // --- Metody pomocnicze ---

    void InitializeNeat()
    {
        // Tutaj umieść kod inicjalizujący Twój algorytm NEAT
        // np. tworzenie początkowej populacji genomów
        Debug.Log("Implementacja InitializeNeat() jest wymagana.");
        // Przykład (zależy od biblioteki):
        // var genomeFactory = new NeatGenomeFactory(...); // Skonfiguruj factory
        // neatAlgorithm = NeatEvolutionAlgorithm<NeatGenome>.Create(...) // Skonfiguruj algorytm
        // neatAlgorithm.InitializeEvolution(genomeFactory, populationSize);
        // currentPopulation = neatAlgorithm.Population;
        // populationSize = currentPopulation.Count; // Upewnij się co do rozmiaru
    }

    void EvolveNextGeneration()
    {
        // Tutaj umieść kod wykonujący krok ewolucji NEAT
        // (selekcja, krzyżowanie, mutacja)
        Debug.Log("Implementacja EvolveNextGeneration() jest wymagana.");
        // Przykład (zależy od biblioteki):
        // neatAlgorithm.PerformOneGeneration();
        // currentPopulation = neatAlgorithm.Population;
    }

    Vector3 GetStartPosition()
    {
        // Zwróć pozycję startową dla drona
        // Może to być stała wartość, losowa w pewnym zakresie,
        // lub odczytana z obiektu w scenie.
        return new Vector3(0, 1, 0); // Przykładowa pozycja
    }

    // --- Komponenty wymagane przez drona ---
    // Upewnij się, że masz skrypt DroneController na swoim prefabie drona.
    // Powinien on zawierać logikę:
    // - Odbierania danych z sensorów (wejścia do sieci neuronowej)
    // - Sterowania dronem na podstawie wyjść sieci neuronowej
    // - Obliczania fitnessu na podstawie zadania (np. dystans do celu, czas w powietrzu, unikanie przeszkód)
    // - Metodę AssignNetwork(genome) do przypisania sieci neuronowej.
    // - Metodę CalculateFitness() zwracającą wynik drona.
    // - Metodę lub flagę IsFinished() wskazującą, czy symulacja dla tego drona powinna się zakończyć.
}

// --- Przykładowy szkielet DroneController ---
/*
public class DroneController : MonoBehaviour
{
    private object neuralNetwork; // Typ zależy od biblioteki NEAT (np. IBlackBox)
    private bool isFinished = false;
    private float currentFitness = 0f;

    public void AssignNetwork(object network)
    {
        this.neuralNetwork = network;
        // Zainicjuj stan drona/sieci
    }

    void FixedUpdate() // Użyj FixedUpdate dla fizyki
    {
        if (neuralNetwork == null || isFinished) return;

        // 1. Zbierz dane z sensorów (np. Raycast, pozycja, prędkość)
        // double[] inputs = GatherSensorData();

        // 2. Aktywuj sieć neuronową
        // IBlackBox box = neuralNetwork as IBlackBox;
        // box.InputSignalArray.CopyFrom(inputs, 0);
        // box.Activate();
        // double[] outputs = new double[box.OutputSignalArray.Length];
        // box.OutputSignalArray.CopyTo(outputs, 0);

        // 3. Zastosuj wyjścia do sterowania dronem (np. siły do Rigidbody)
        // ApplyControls(outputs);

        // 4. Aktualizuj fitness w trakcie symulacji
        // UpdateFitness();

        // 5. Sprawdź warunki zakończenia (kolizja, osiągnięcie celu, timeout wewnętrzny)
        // CheckFinishConditions();
    }

    private void UpdateFitness()
    {
        // Przykładowo: nagradzaj za zbliżanie się do celu, karaj za bezczynność
        // currentFitness += Time.fixedDeltaTime * (CalculateReward());
    }

    private void CheckFinishConditions()
    {
        // Np. jeśli dron zderzył się z czymś
        // if (collided) { isFinished = true; }
        // Np. jeśli dron osiągnął cel
        // if (reachedTarget) { isFinished = true; currentFitness += 1000; } // Duża nagroda
    }

    public float CalculateFitness()
    {
        // Zwróć ostateczny obliczony fitness
        return currentFitness;
    }

    public bool IsFinished()
    {
        return isFinished;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Przykładowe zakończenie po kolizji
        // isFinished = true;
        // currentFitness -= 50; // Kara za kolizję
    }
}
*/