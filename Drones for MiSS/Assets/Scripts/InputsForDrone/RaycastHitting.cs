using Assets.Scripts;
using UnityEngine;
using UnityEngine.Rendering;

public class RaycastHitting : MonoBehaviour, IGetInputs
{
    public int horizontalRays = 2;
    public int verticalRays = 3;
    public float horizontalAngle = 60f;
    public float verticalAngle = 120f;
    public float sensorMaxDistance = 200f;

    private int frameCounter;

    public double[] ConeRaycastSensors(Transform droneTransform, float maxDistance, int horizontalRays, int verticalRays, float horizontalAngle, float verticalAngle)
    {
        double[] neuronValues = new double[horizontalRays * verticalRays * 5];
        

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
                double isObstacle = 0;
                double isForClean = 0;
                double isMovable = 0;
                double distance = maxDistance;

                float horizontalOffset = horizontalStart + h * horizontalStep;

                Vector3 direction = Quaternion.Euler(verticalOffset, horizontalOffset, 0) * droneTransform.forward;

                if (Physics.Raycast(droneTransform.position, direction, out RaycastHit hit, maxDistance))
                {

                    TypeOfObstacle typeOfObstacle = hit.collider.GetComponent<TypeOfObstacle>();
                    if (typeOfObstacle != null)
                    {
                        isObstacle = typeOfObstacle.IsObstacle;
                        isForClean = typeOfObstacle.IsForClean;
                        isMovable = typeOfObstacle.IsMovable;
                    }
                    distance = hit.distance;
                }
               

                neuronValues[index] = distance;
                neuronValues[index + 1] = isObstacle;
                neuronValues[index + 2] = isForClean;
                neuronValues[index + 3] = isMovable;
                neuronValues[index + 4] = 0.0;


                index += 4;
            }
        }


        return neuronValues;
    }

    public double[] GetInputs()
    {
        
        double[] neuronValues = ConeRaycastSensors(transform, sensorMaxDistance, horizontalRays, verticalRays, horizontalAngle, verticalAngle);
        if (frameCounter % 500 == 0)
        {
            for (int i = 0; i < neuronValues.Length; i++)
            {
                Debug.Log(neuronValues[i]);
            }
        }
        
        frameCounter++;
        return neuronValues;
        
        
    }

    public int GetInputsCount()
    {
        return horizontalRays * verticalRays * 4;
    }
}
