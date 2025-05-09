using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
using Assets.Scripts;

// Ten skrypt symuluje naciśnięcia na Keyboard.current.
// Ma działać W POŁĄCZENIU ze skonfigurowanym komponentem PlayerInput
// dodanym do obiektu drona w scenie.
public class DroneSim3 : MonoBehaviour
{
    [Tooltip("Czas, przez jaki symulowany klawisz pozostaje wciśnięty podczas jednego 'kliknięcia' w sekundach.")]
    public float clickDuration = 0.1f;

    // Kontroler zwracający DroneMove
    private IMakeAction controller = new RandomController();
    private Coroutine activeClickCoroutine = null;

    void Awake()
    {
        // Inicjalizacja kontrolera (bez zmian)
        if (controller == null)
        {
            controller = new RandomController();
            Debug.Log("Używam domyślnego RandomController zwracającego DroneMove.");
        }
    }

    void Start()
    {
        // Sprawdzenie dostępności klawiatury (bez zmian)
        if (Keyboard.current == null)
        {
            Debug.LogError("[InputSystem] Nie wykryto systemowej klawiatury (Keyboard.current jest null)! " +
                           "Symulacja na niej nie będzie możliwa.");
        }
        else
        {
            Debug.Log($"[InputSystem] Będę próbował symulować zdarzenia na systemowej klawiaturze: {Keyboard.current.displayName}");
        }
    }

    /// <summary>
    /// Wykonuje pojedyncze symulowane "kliknięcie" klawisza na Keyboard.current.
    /// </summary>
    public void ClickKey()
    {
        if (activeClickCoroutine != null)
        {
            StopCoroutine(activeClickCoroutine);
            Keyboard currentKb = Keyboard.current;
            if (currentKb != null)
            {
                ReleaseKeyOnSystemKeyboard(Key.None, currentKb); // Key.None użyte symbolicznie
                Debug.LogWarning("[InputSystem] Przerwano poprzednie kliknięcie na Keyboard.current.");
            }
            activeClickCoroutine = null;
        }
        activeClickCoroutine = StartCoroutine(PerformClickAction());
    }

    // Korutyna wykonująca pojedynczą sekwencję Press -> Wait -> Release na Keyboard.current
    private IEnumerator PerformClickAction()
    {
        Keyboard systemKeyboard = Keyboard.current;
        if (systemKeyboard == null)
        {
            Debug.LogError("[InputSystem] Keyboard.current jest null! Nie można wykonać kliknięcia.");
            activeClickCoroutine = null;
            yield break;
        }

        DroneMove move = controller.MakeAction();
        Key keyToSimulate = DroneMoveKeyMapping.GetKey(move);

        if (keyToSimulate != Key.None)
        {
            PressKeyOnSystemKeyboard(keyToSimulate, systemKeyboard);
            // Log informujący o symulacji jest ważny
            Debug.Log($"[InputSystem] Click Press on System Keyboard ({systemKeyboard.name}): {keyToSimulate} (dla ruchu: {move})");

            yield return new WaitForSeconds(clickDuration);

            systemKeyboard = Keyboard.current; // Sprawdź ponownie
            if (systemKeyboard == null)
            {
                Debug.LogWarning("[InputSystem] Keyboard.current stał się null podczas oczekiwania!");
                activeClickCoroutine = null;
                yield break;
            }

            ReleaseKeyOnSystemKeyboard(keyToSimulate, systemKeyboard);
            Debug.Log($"[InputSystem] Click Release on System Keyboard ({systemKeyboard.name}): {keyToSimulate} (dla ruchu: {move})");
        }
        else
        {
            Debug.Log($"[InputSystem] Ruch {move} nie mapuje się na żaden klawisz (Idle?). Nic nie klikam.");
        }
        activeClickCoroutine = null;
    }

    // Metoda pomocnicza do naciśnięcia klawisza na podanym urządzeniu Keyboard
    private void PressKeyOnSystemKeyboard(Key key, Keyboard device)
    {
        var keyState = new KeyboardState(key);
        InputSystem.QueueStateEvent(device, keyState);
    }

    // Metoda pomocnicza do zwolnienia klawisza na podanym urządzeniu Keyboard
    private void ReleaseKeyOnSystemKeyboard(Key key, Keyboard device)
    {
        var keyState = new KeyboardState();
        InputSystem.QueueStateEvent(device, keyState);
    }

    void OnDestroy()
    {
        if (activeClickCoroutine != null)
        {
            StopCoroutine(activeClickCoroutine);
            activeClickCoroutine = null;
             Keyboard currentKb = Keyboard.current;
             if(currentKb != null) {
                ReleaseKeyOnSystemKeyboard(Key.None, currentKb);
             }
        }
    }

    // Upewnij się, że masz też definicje DroneMove, IMakeAction, RandomController i DroneKeyMapper w projekcie
}