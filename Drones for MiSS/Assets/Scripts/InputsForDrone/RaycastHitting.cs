using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class RaycastHitting : MonoBehaviour, IGetInputs
{
    // --- Publiczna konfiguracja dla siatki sensorów 3x2 ---
    [Tooltip("K¹t rozproszenia dla bocznych sensorów w p³aszczyŸnie poziomej (w stopniach).")]
    [Range(0, 90)]
    public float horizontalSpreadAngle = 30f;

    [Tooltip("K¹t nachylenia dla górnego i dolnego poziomu sensorów (w stopniach).")]
    [Range(0, 90)]
    public float verticalLevelAngle = 15f;

    [Tooltip("Maksymalny dystans wykrywania dla wszystkich sensorów.")]
    public float sensorMaxDistance = 200f;

    // --- Prywatne pola ---
    private List<Sensor> sensors; // Lista sensorów, która zostanie wype³niona przez fabrykê
    private int frameCounter;

    /// <summary>
    /// Wywo³ywane podczas ³adowania instancji skryptu.
    /// U¿ywamy fabryki do stworzenia naszej konfiguracji sensorów.
    /// </summary>
    private void Awake()
    {
        InitializeSensors();
    }

    /// <summary>
    /// Inicjalizuje listê sensorów, wywo³uj¹c metodê z fabryki.
    /// </summary>
    private void InitializeSensors()
    {
        // Wywo³ujemy fabrykê, aby stworzy³a dla nas listê sensorów w konfiguracji 3x2.
        //sensors = SensorFactory.CreateGrid6(horizontalSpreadAngle, verticalLevelAngle);
        sensors = SensorFactory.CreateDroneSensors9(horizontalSpreadAngle, verticalLevelAngle);
    }

    /// <summary>
    /// G³ówna metoda dla interfejsu IGetInputs.
    /// </summary>
    public double[] GetInputs()
    {
        if (sensors == null || sensors.Count == 0)
        {
            Debug.LogError("Sensory nie zosta³y zainicjalizowane!");
            return new double[0];
        }

        double[] neuronValues = new double[sensors.Count * 5];
        int index = 0;

        foreach (var sensor in sensors)
        {
            sensor.Update(transform, sensorMaxDistance);

            neuronValues[index] = sensor.Distance;
            neuronValues[index + 1] = sensor.IsObstacle;
            neuronValues[index + 2] = sensor.IsForClean;
            neuronValues[index + 3] = sensor.IsMovable;
            neuronValues[index + 4] = sensor.IsNotForClean;

            index += 5;
        }

        if (frameCounter > 0 && frameCounter % 500 == 0)
        {
            Debug.Log($"Ramka {frameCounter}: Zebrano {neuronValues.Length} danych wejœciowych.");
        }

        frameCounter++;
        return neuronValues;
    }

    /// <summary>
    /// Zwraca ca³kowit¹ liczbê wartoœci wejœciowych.
    /// </summary>
    public int GetInputsCount()
    {
        return sensors != null ? sensors.Count * 5 : 0;
    }

    /// <summary>
    /// Wizualizacja sensorów w edytorze Unity.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Jeœli jesteœmy w edytorze, chcemy widzieæ zmiany na ¿ywo.
        // Ta linia zapewni, ¿e gizmosy bêd¹ siê aktualizowaæ przy zmianie parametrów w inspektorze.
#if UNITY_EDITOR
        if (sensors == null || !Application.isPlaying)
        {
            InitializeSensors();
        }
#endif

        if (sensors == null) return;

        foreach (var sensor in sensors)
        {
            sensor.DrawGizmo(transform, sensorMaxDistance);
        }
    }
}
