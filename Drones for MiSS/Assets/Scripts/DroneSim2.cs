using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
using Assets.Scripts;

public class DroneSim2 : MonoBehaviour
{
    [Tooltip("Czas, przez jaki symulowany klawisz pozostaje wciśnięty podczas jednego 'kliknięcia' w sekundach.")]
    public float clickDuration = 0.1f; // Krótki czas wciśnięcia dla symulacji kliknięcia

    // Możesz przypisać inny kontroler w Inspektorze
    private IMakeAction controller;

    private Keyboard virtualKeyboard; // Nasza wirtualna klawiatura
    private Coroutine activeClickCoroutine = null; // Śledzi aktywną korutynę kliknięcia

    void Awake()
    {
        // Inicjalizuj kontroler (np. domyślnie losowy)
        if (controller == null)
        {
            controller = new RandomController();
            Debug.Log("Używam domyślnego RandomController.");
        }
    }

    void Start()
    {
        // Znajdź lub stwórz wirtualną klawiaturę
        InitializeVirtualKeyboard();

        // Usunęliśmy start ciągłej korutyny symulacji
        // StartCoroutine(SimulateKeyPressCoroutine());
    }

    private void InitializeVirtualKeyboard()
    {
        virtualKeyboard = InputSystem.GetDevice<Keyboard>("VirtualKeyboard");
        if (virtualKeyboard == null)
        {
            virtualKeyboard = InputSystem.AddDevice<Keyboard>("VirtualKeyboard");
            Debug.Log("Stworzono wirtualną klawiaturę.");
        }
        else if (!virtualKeyboard.added)
        {
            InputSystem.AddDevice(virtualKeyboard);
            Debug.Log("Dodano istniejącą wirtualną klawiaturę do systemu.");
        }
        else
        {
            Debug.Log("Znaleziono istniejącą wirtualną klawiaturę.");
        }
    }

    /// <summary>
    /// Wykonuje pojedyncze symulowane "kliknięcie" klawisza.
    /// Wybiera akcję, naciska odpowiedni klawisz na wirtualnej klawiaturze,
    /// czeka przez 'clickDuration' i zwalnia klawisz.
    /// </summary>
    public void ClickKey()
    {
        // Jeśli poprzednie kliknięcie jeszcze trwa, można je przerwać lub poczekać.
        // Tutaj prosta implementacja: przerywamy poprzednie, jeśli istnieje.
        if (activeClickCoroutine != null)
        {
            StopCoroutine(activeClickCoroutine);
            // Upewnij się, że klawisz z poprzedniego kliknięcia został zwolniony,
            // wysyłając pusty stan (najprostszy sposób)
            if (virtualKeyboard != null && virtualKeyboard.added)
            {
                InputSystem.QueueStateEvent(virtualKeyboard, new KeyboardState());
                Debug.LogWarning("[InputSystem] Przerwano poprzednie kliknięcie.");
            }
            activeClickCoroutine = null;

        }

        // Rozpocznij nową korutynę dla pojedynczego kliknięcia
        activeClickCoroutine = StartCoroutine(PerformClickAction());
    }

    // Korutyna wykonująca pojedynczą sekwencję Press -> Wait -> Release
    private IEnumerator PerformClickAction()
    {
        // 1. Sprawdź, czy wirtualna klawiatura jest gotowa
        if (virtualKeyboard == null || !virtualKeyboard.added)
        {
            Debug.LogError("[InputSystem] Wirtualna klawiatura nie jest gotowa!");
            activeClickCoroutine = null; // Zakończ śledzenie korutyny
            yield break; // Zakończ korutynę
        }

        // 2. Pobierz akcję od kontrolera
        DroneMove action = controller.MakeAction();

        // 3. Zmapuj akcję na klawisz
        Key keyToSimulate = DroneMoveKeyMapping.GetKey(action);

        // 4. Sprawdź, czy mamy poprawny klawisz
        if (keyToSimulate != Key.None)
        {
            // 5. Symuluj naciśnięcie klawisza
            PressKeyOnVirtualKeyboard(keyToSimulate);
            Debug.Log($"[InputSystem] Click Press: {keyToSimulate}");

            // 6. Poczekaj określony czas 'przytrzymania' dla kliknięcia
            yield return new WaitForSeconds(clickDuration);

            // Dodatkowe sprawdzenie, czy w międzyczasie klawiatura nie zniknęła
            if (virtualKeyboard == null || !virtualKeyboard.added)
            {
                Debug.LogWarning("[InputSystem] Wirtualna klawiatura zniknęła podczas kliknięcia!");
                activeClickCoroutine = null;
                yield break;
            }

            // 7. Symuluj zwolnienie klawisza
            ReleaseKeyOnVirtualKeyboard(keyToSimulate);
            Debug.Log($"[InputSystem] Click Release: {keyToSimulate}");
        }
        else
        {
            Debug.Log("[InputSystem] Akcja nie mapuje się na żaden klawisz (Idle?). Nic nie klikam.");
        }

        // Zakończono kliknięcie, zresetuj śledzenie korutyny
        activeClickCoroutine = null;
    }

    // Funkcja pomocnicza do naciśnięcia klawisza
    private void PressKeyOnVirtualKeyboard(Key key)
    {
        if (virtualKeyboard == null || !virtualKeyboard.added) return;
        var keyState = new KeyboardState(key);
        InputSystem.QueueStateEvent(virtualKeyboard, keyState);
    }

    // Funkcja pomocnicza do zwolnienia klawisza
    private void ReleaseKeyOnVirtualKeyboard(Key key)
    {
        // Używamy key jako argumentu tylko dla logów,
        // bo wysłanie pustego stanu zwalnia *wszystkie* klawisze wirtualne.
        if (virtualKeyboard == null || !virtualKeyboard.added) return;
        var keyState = new KeyboardState(); // Pusty stan resetuje wszystkie klawisze
        InputSystem.QueueStateEvent(virtualKeyboard, keyState);
    }

    void OnDestroy()
    {
        // Przerwij korutynę, jeśli nadal działa
        if (activeClickCoroutine != null)
        {
            StopCoroutine(activeClickCoroutine);
            activeClickCoroutine = null;
        }

        // Usuń wirtualną klawiaturę
        if (virtualKeyboard != null && virtualKeyboard.added)
        {
            // Dobrą praktyką jest zwolnienie wszystkich klawiszy przed usunięciem
            InputSystem.QueueStateEvent(virtualKeyboard, new KeyboardState());
            InputSystem.RemoveDevice(virtualKeyboard);
            Debug.Log("Usunięto wirtualną klawiaturę.");
        }

        // Ewentualne zamknięcie PythonEngine, jeśli używane
        // Python.Runtime.PythonEngine.Shutdown();
    }

    /*
    // Usunięta pętla ciągłej symulacji
    private IEnumerator SimulateKeyPressCoroutine()
    {
        // ... KOD USUNIĘTY ...
    }
    */

    // Ewentualne OnApplicationQuit dla PythonEngine
    /*
    private void OnApplicationQuit()
    {
       Python.Runtime.PythonEngine.Shutdown();
    }
    */
}