using System.Collections.Generic;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Dictionary<string, float> totalCollisionTimeByName = new Dictionary<string, float>();
    // S³ownik przechowuj¹cy listê czasów rozpoczêcia kolizji dla obiektu o danej nazwie
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

            Debug.Log("Kolizja z obiektem " + name + " trwa³a: " + duration + " sekund. £¹czny czas: " + totalCollisionTimeByName[name] + " sekund.");
        }
    }
}
