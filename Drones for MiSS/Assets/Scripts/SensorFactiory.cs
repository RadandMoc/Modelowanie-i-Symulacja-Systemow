using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static factory class for creating predefined lists of sensors.
/// This helps to keep the sensor configuration logic separate from the main RaycastHitting class.
/// </summary>
public static class SensorFactory
{
    /// <summary>
    /// Creates a 3x2 grid of 6 sensors.
    /// The layout consists of two horizontal levels (upper and lower).
    /// Each level has three sensors: left, center, and right.
    /// </summary>
    /// <param name="horizontalSpreadAngle">The angle between the center and side sensors on the horizontal plane.</param>
    /// <param name="verticalLevelAngle">The vertical angle for the upper and lower sensor levels.</param>
    /// <returns>A list containing 6 configured sensors.</returns>
    public static List<Sensor> CreateGrid6(float horizontalSpreadAngle, float verticalLevelAngle)
    {
        var sensors = new List<Sensor>();

        // Define horizontal angles for left, center, and right sensors
        float[] horizontalAngles = { -horizontalSpreadAngle, 0f, horizontalSpreadAngle };

        // Define vertical angles for lower and upper levels
        float[] verticalAngles = { -verticalLevelAngle, verticalLevelAngle };

        // Create sensors for each defined angle combination
        foreach (float vAngle in verticalAngles)
        {
            foreach (float hAngle in horizontalAngles)
            {
                // Calculate the direction for this sensor relative to the drone's forward vector
                Vector3 direction = Quaternion.Euler(vAngle, hAngle, 0) * Vector3.forward;

                // Create a new sensor and add it to the list
                sensors.Add(new Sensor(direction));
            }
        }

        return sensors;
    }

    /// <summary>
    /// (Example for the future) Creates a cone-shaped sensor array.
    /// </summary>
    /// <param name="horizontalRays">Number of rays horizontally.</param>
    /// <param name="verticalRays">Number of rays vertically.</param>
    /// <param name="coneAngleH">Total horizontal angle of the cone.</param>
    /// <param name="coneAngleV">Total vertical angle of the cone.</param>
    /// <returns>A list of sensors arranged in a cone.</returns>
    public static List<Sensor> CreateCone(int horizontalRays, int verticalRays, float coneAngleH, float coneAngleV)
    {
        var sensors = new List<Sensor>();

        // --- Logic from the first version of the code could be placed here ---
        // This demonstrates how you can easily add more configurations.

        Debug.LogWarning("CreateCone method is not fully implemented yet.");

        return sensors;
    }
}
