using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DroneKinematics : MonoBehaviour, IGetInputs
{
    // Zmienne do przechowywania stanu między wywołaniami GetInputs()
    // na potrzeby obliczenia prędkości
    private Vector3 lastPositionForVelocityCalculation;
    private float lastTimeVelocityWasCalculated;
    private Vector3 lastCalculatedVelocity; // Przechowuje ostatnio obliczoną prędkość
    private const int neurons = 9;


    void Awake()
    {
        // Inicjalizacja stanu potrzebnego do obliczenia prędkości
        lastPositionForVelocityCalculation = transform.position;
        // Użyj Time.time, które jest skalowane przez Time.timeScale.
        // Jeśli potrzebujesz czasu, który płynie nawet gdy gra jest spauzowana (Time.timeScale = 0),
        // rozważ Time.unscaledTime.
        lastTimeVelocityWasCalculated = Time.time;
        lastCalculatedVelocity = Vector3.zero; // Początkowa prędkość drona to zero

        // Komunikat informacyjny
        Debug.Log($"Dron '{gameObject.name}': Inicjalizacja DroneKinematics. Prędkość będzie obliczana na żądanie w metodzie GetInputs().");
    }

    // Metoda Update() nie jest już potrzebna do obliczania prędkości w tej klasie

    /// <summary>
    /// Zwraca tablicę double zawierającą dane kinematyczne drona:
    /// Pozycja (x, y, z), Prędkość (x, y, z), Obrót (Kąty Eulera x, y, z).
    /// Prędkość jest obliczana na podstawie zmiany pozycji od ostatniego wywołania tej metody.
    /// </summary>
    public double[] GetInputs()
    {
        Vector3 currentPosition = transform.position;
        Vector3 currentRotationEuler = transform.eulerAngles;
        Vector3 calculatedVelocityNow;

        //
        //Debug.Log(currentPosition);

        float currentTime = Time.time;
        float deltaTimeSinceLastCall = currentTime - lastTimeVelocityWasCalculated;

        if (deltaTimeSinceLastCall > 0.0001f) 
        {
            calculatedVelocityNow = (currentPosition - lastPositionForVelocityCalculation) / deltaTimeSinceLastCall;
            lastCalculatedVelocity = calculatedVelocityNow;
        }
        else
        {
            
            calculatedVelocityNow = lastCalculatedVelocity;
        }

        lastPositionForVelocityCalculation = currentPosition;
        lastTimeVelocityWasCalculated = currentTime;

        double[] inputs = new double[GetInputsCount()];

        inputs[0] = (double)currentPosition.x;
        inputs[1] = (double)currentPosition.y;
        inputs[2] = (double)currentPosition.z;

        inputs[3] = (double)calculatedVelocityNow.x;
        inputs[4] = (double)calculatedVelocityNow.y;
        inputs[5] = (double)calculatedVelocityNow.z;

        inputs[6] = (double)currentRotationEuler.x;
        inputs[7] = (double)currentRotationEuler.y;
        inputs[8] = (double)currentRotationEuler.z;

        return inputs;
    }

    /// <summary>
    /// Zwraca całkowitą liczbę wartości wejściowych dostarczanych przez tę klasę.
    /// </summary>
    public int GetInputsCount()
    {
        return neurons;
    }
}