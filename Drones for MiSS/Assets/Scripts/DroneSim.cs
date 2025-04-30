using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using Python.Runtime;
using System.IO;
using Assets.Scripts;
public class DroneSim : MonoBehaviour
{
    
    public float interval = 2f;
    public float holdTime = 1f;
    private bool isHeadless;
    private IMakeAction controller = new RandomController();

    private void Start()
    {
        StartCoroutine(RandomKeyPressCoroutine());
    }

    private IEnumerator RandomKeyPressCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Wybierz losowy klawisz
            byte key =  DroneMoveKeyMapping.GetKeyCode(controller.MakeAction());

            if (isHeadless)
            {
                KeyboardSimulator.PressKey(key);
                Debug.Log($"[Headless] Press {key:X2}");

                yield return new WaitForSeconds(holdTime);

                KeyboardSimulator.ReleaseKey(key);
                Debug.Log($"[Headless] Release {key:X2}");
            }
            else
            {
                // Tryb GUI: tutaj mo¿esz u¿yæ innej metody, np. symulowaæ zdarzenia wejœcia lub korzystaæ z fizycznej klawiatury.
                // W tym przyk³adzie dla uproszczenia wywo³ujemy te same metody, ale mo¿esz to zmodyfikowaæ.
                KeyboardSimulator.PressKey(key);
                Debug.Log($"[GUI] Simulated Press {key:X2}");

                yield return new WaitForSeconds(holdTime);

                KeyboardSimulator.ReleaseKey(key);
                Debug.Log($"[GUI] Simulated Release {key:X2}");
            }
        }
    }
    private void OnApplicationQuit()
    {
        PythonEngine.Shutdown();
    }
}