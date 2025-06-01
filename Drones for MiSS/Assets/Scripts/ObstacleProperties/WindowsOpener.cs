using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class WindowsOpener : MonoBehaviour
{

    bool isOpen = false;

    public void RotateWindow(float angle, float duration)
    {
        float rotationAngle = isOpen ? -angle : angle;
        StartCoroutine(RotateOverTime(rotationAngle, duration));
        isOpen = !isOpen;
    }

    private IEnumerator RotateOverTime(float angle, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = endRotation;
    }

}

