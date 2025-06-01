using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public class WindowsManager : MonoBehaviour
{
    public List<WindowsOpener> windows;
    int i = 0;
    int idx = 0;
    float angle = 90f;
    float duration = 2f;

    private void Update()
    {
        if (i % 1000 == 0) 
        {
            windows[idx % windows.Count].RotateWindow(angle, duration);
            idx ++;
        }
        i++;
    }

}

