using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using Python.Runtime;
using System.IO;
public class DroneSim : MonoBehaviour
{
    private byte[] keys = new byte[]
    {
        0x57, // W
        0x41, // A
        0x53, // S
        0x44, // D
        0x49, // I
        0x4A, // J
        0x4B, // K
        0x4C  // L
    };

    public float interval = 2f;
    public float holdTime = 1f;
    private bool isHeadless;

    private void Start()
    {
        isHeadless = Application.isBatchMode;

        PythonEngine.Initialize();

        using (Py.GIL())
        {
			dynamic a = 5;
            Debug.Log(5);
			/*
            string projectRoot = Path.GetFullPath(Application.dataPath + "/..");
            string sitePackagesPath = Path.Combine(projectRoot, "venv", "Lib", "site-packages");
            dynamic sys = Py.Import("sys");
            sys.path.append(sitePackagesPath);

            string pythonScriptsFolder = System.IO.Path.Combine(projectRoot, "PythonScripts");

            sys.path.append(pythonScriptsFolder);

            dynamic testScript = Py.Import("test_script");
            testScript.main();
			*/
        }


        if (isHeadless)
        {
            Debug.Log("Tryb headless - wstrzykiwanie klawiszy.");
        }
        else
        {
            Debug.Log("Tryb GUI - korzystanie z fizycznej klawiatury lub symulacja w inny sposób.");
        }

        StartCoroutine(RandomKeyPressCoroutine());
    }

    private IEnumerator RandomKeyPressCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Wybierz losowy klawisz
            byte randomKey = keys[Random.Range(0, keys.Length)];

            if (isHeadless)
            {
                KeyboardSimulator.PressKey(randomKey);
                Debug.Log($"[Headless] Press {randomKey:X2}");

                yield return new WaitForSeconds(holdTime);

                KeyboardSimulator.ReleaseKey(randomKey);
                Debug.Log($"[Headless] Release {randomKey:X2}");
            }
            else
            {
                // Tryb GUI: tutaj mo¿esz u¿yæ innej metody, np. symulowaæ zdarzenia wejœcia lub korzystaæ z fizycznej klawiatury.
                // W tym przyk³adzie dla uproszczenia wywo³ujemy te same metody, ale mo¿esz to zmodyfikowaæ.
                KeyboardSimulator.PressKey(randomKey);
                Debug.Log($"[GUI] Simulated Press {randomKey:X2}");

                yield return new WaitForSeconds(holdTime);

                KeyboardSimulator.ReleaseKey(randomKey);
                Debug.Log($"[GUI] Simulated Release {randomKey:X2}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        PythonEngine.Shutdown();
    }
}