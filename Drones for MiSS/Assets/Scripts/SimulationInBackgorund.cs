using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class SimulationInBackgorund : MonoBehaviour
    {
        void Awake()
        {
            // Ta linia sprawia, że aplikacja będzie działać
            // w tle z pełną prędkością i nie będzie pauzować.
            Application.runInBackground = true;

            // Opcjonalnie: Możesz też ustawić stałą liczbę klatek,
            // aby symulacja była bardziej deterministyczna (jeśli jest to potrzebne).
            // Np. Application.targetFrameRate = 60;
        }
    }
    
}
