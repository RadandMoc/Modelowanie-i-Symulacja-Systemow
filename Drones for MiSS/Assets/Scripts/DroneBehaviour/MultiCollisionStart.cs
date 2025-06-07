using System.Collections.Generic;
using UnityEngine;

public class MultiCollisionBehaviour : MonoBehaviour
{
    private Dictionary<string, float> totalCollisionTimeByName = new Dictionary<string, float>();

    private Dictionary<string, List<float>> collisionStartTimesByName = new Dictionary<string, List<float>>();

    private void OnCollisionEnter(Collision collision)
    {
        string name = collision.gameObject.name;
        if (!collisionStartTimesByName.ContainsKey(name))
        {
            collisionStartTimesByName[name] = new List<float>();
        }

        collisionStartTimesByName[name].Add(Time.time);
    }

    private void OnCollisionExit(Collision collision)
    {
        string name = collision.gameObject.name;


        if (collisionStartTimesByName.ContainsKey(name) && collisionStartTimesByName[name].Count > 0)
        {
            float startTime = collisionStartTimesByName[name][0];
            collisionStartTimesByName[name].RemoveAt(0);

            float duration = Time.time - startTime;

            if (!totalCollisionTimeByName.ContainsKey(name))
            {
                totalCollisionTimeByName[name] = 0f;
            }
            totalCollisionTimeByName[name] += duration;

            Debug.Log("Kolizja z obiektem " + name + " zakoñczy³a siê. Trwa³a: " + duration.ToString("F2") + "s. £¹czny czas: " + totalCollisionTimeByName[name].ToString("F2") + "s.");
        }
    }


    public float CalculateAllCollisionTime()
    {
        float totalTime = 0f;
        foreach (float completedDuration in totalCollisionTimeByName.Values)
        {
            totalTime += completedDuration;
			Debug.Log(completedDuration);
        }


        float currentTime = Time.time;

        foreach (var entry in collisionStartTimesByName)
        {

            foreach (float startTime in entry.Value)
            {
                totalTime += (currentTime - startTime);
				Debug.Log(totalTime);

            }
        }

        return totalTime;
    }
}