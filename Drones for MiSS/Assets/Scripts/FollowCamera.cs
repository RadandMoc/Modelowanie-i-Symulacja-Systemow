using UnityEngine;

/// <summary>
/// Ten skrypt sprawia, że kamera płynnie podąża za celem (target),
/// zachowując początkową odległość i pozycję względem niego.
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("Ustawienia Kamery")]
    [Tooltip("Obiekt, za którym kamera ma podążać. Przeciągnij tu swojego drona.")]
    public Transform target;

    [Tooltip("Jak płynnie kamera ma podążać za celem. Mniejsza wartość = wolniejszy, bardziej 'miękki' ruch.")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f;

    // Prywatne pole przechowujące stałą odległość kamery od celu.
    private Vector3 offset;

    /// <summary>
    /// Metoda Start jest wywoływana raz, na początku.
    /// Obliczamy w niej początkową odległość kamery od celu.
    /// </summary>
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Nie przypisano celu (target) do kamery! Przeciągnij obiekt drona na pole 'Target' w inspektorze.");
            return;
        }

        // Oblicz i zapamiętaj początkową odległość i pozycję kamery względem celu.
        offset = transform.position - target.position;
    }

    /// <summary>
    /// Metoda LateUpdate jest wywoływana w każdej klatce, ale PO wykonaniu wszystkich metod Update.
    /// Jest to najlepsze miejsce na logikę kamery, aby uniknąć "drgania" obrazu.
    /// </summary>
    void LateUpdate()
    {
        // Jeśli cel nie istnieje (np. został zniszczony), nie rób nic.
        if (target == null) return;

        // 1. Oblicz docelową pozycję kamery w tej klatce.
        Vector3 desiredPosition = target.position + offset;

        // 2. Użyj interpolacji liniowej (Lerp) do płynnego przesunięcia kamery.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 3. Zaktualizuj pozycję kamery.
        transform.position = smoothedPosition;

        // 4. (Opcjonalnie) Upewnij się, że kamera zawsze patrzy na cel.
        // Odkomentuj poniższą linię, jeśli chcesz, aby kamera obracała się razem z celem.
        // transform.LookAt(target);
    }
}