using UnityEngine;
using UnityEngine.Rendering;

public class RaycastHitting : MonoBehaviour
{
    public int horizontalRays = 10;
    public int verticalRays = 6;
    public float horizontalAngle = 60f;
    public float verticalAngle = 120f;
    public float sensorMaxDistance = 200f;

    private int frameCounter;

    void Start()
    {
        frameCounter = 0;
    }

    void Update()
    {
        frameCounter++;

        if (frameCounter % 10 == 0)
        {
            float[] sensorReadings = ConeRaycastSensors(transform, sensorMaxDistance, horizontalRays, verticalRays, horizontalAngle, verticalAngle);

            if (frameCounter % 90 == 0)
            {
                Debug.Log("Pierwszy sensor: " + sensorReadings[0]);
            }
        }
    }


    public float[] ConeRaycastSensors(Transform droneTransform, float maxDistance, int horizontalRays, int verticalRays, float horizontalAngle, float verticalAngle)
    {
        float[] sensorValues = new float[horizontalRays * verticalRays];

        float horizontalStart = -horizontalAngle / 2f;
        float verticalStart = -verticalAngle / 2f;

        float horizontalStep = horizontalAngle / (horizontalRays - 1);
        float verticalStep = verticalAngle / (verticalRays - 1);

        int index = 0;
        for (int v = 0; v < verticalRays; v++)
        {
            float verticalOffset = verticalStart + v * verticalStep;

            for (int h = 0; h < horizontalRays; h++)
            {
                float horizontalOffset = horizontalStart + h * horizontalStep;

                Vector3 direction = Quaternion.Euler(verticalOffset, horizontalOffset, 0) * droneTransform.forward;

                if (Physics.Raycast(droneTransform.position, direction, out RaycastHit hit, maxDistance))
                {
                    sensorValues[index] = hit.distance / maxDistance;
                }
                else
                {
                    sensorValues[index] = 1f;
                }
                if (index % 2 == 0 )
                    Debug.DrawRay(droneTransform.position, direction * maxDistance, Color.green);
                else
                    Debug.DrawRay(droneTransform.position, direction * maxDistance, Color.red);

                index++;
            }
        }

        return sensorValues;
    }


}
