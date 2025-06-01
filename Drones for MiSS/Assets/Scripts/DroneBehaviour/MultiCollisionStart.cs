using System.Collections.Generic;
using UnityEngine;

public class MultiCollisionBehaviour : MonoBehaviour
{
    private Dictionary<string, double> totalCollisionTimeByName = new Dictionary<string, double>();
    // S³ownik przechowuj¹cy listê czasów rozpoczêcia kolizji dla obiektu o danej nazwie
    private Dictionary<string, List<double>> collisionStartTimesByName = new Dictionary<string, List<double>>();

    private void OnCollisionEnter(Collision collision)
    {
        string name = collision.gameObject.name;
        if (!collisionStartTimesByName.ContainsKey(name))
        {
            collisionStartTimesByName[name] = new List<double>();
        }
        collisionStartTimesByName[name].Add(Time.time);
    }

    private void OnCollisionExit(Collision collision)
    {
        string name = collision.gameObject.name;
        if (collisionStartTimesByName.ContainsKey(name) && collisionStartTimesByName[name].Count > 0)
        {
            double startTime = collisionStartTimesByName[name][0];
            collisionStartTimesByName[name].RemoveAt(0);

            double duration = Time.time - startTime;

            if (!totalCollisionTimeByName.ContainsKey(name))
            {
                totalCollisionTimeByName[name] = 0f;
            }
            totalCollisionTimeByName[name] += duration;

            Debug.Log("Kolizja z obiektem " + name + " trwa³a: " + duration + " sekund. £¹czny czas: " + totalCollisionTimeByName[name] + " sekund.");
        }
    }

    public double CalculateAllCollisionTime() 
    {
        double totalCollisionTime = 0f;
        foreach (var entry in totalCollisionTimeByName)
        {
            totalCollisionTime += entry.Value;
        }
        return totalCollisionTime;
    }
}
