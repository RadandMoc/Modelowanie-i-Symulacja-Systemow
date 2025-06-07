using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Sensor
    {
        // --- Public properties to hold the latest sensor readings ---
        public double Distance { get; private set; }
        public double IsObstacle { get; private set; }
        public double IsForClean { get; private set; }
        public double IsMovable { get; private set; }
        public double IsNotForClean { get; private set; }
        public Vector3 HitPoint { get; private set; } // Store the hit point for debugging/visualization
        public bool DidHit { get; private set; }      // Flag to check if the raycast hit something

        // --- Private fields ---
        private readonly Vector3 _direction; // The local direction of the sensor's raycast

        /// <summary>
        /// Initializes a new sensor with a specific direction.
        /// </summary>
        /// <param name="direction">The direction of the raycast, relative to the drone's forward vector.</param>
        public Sensor(Vector3 direction)
        {
            _direction = direction;
        }

        /// <summary>
        /// Performs a raycast and updates the sensor's properties with the results.
        /// </summary>
        /// <param name="droneTransform">The transform of the drone (origin and orientation).</param>
        /// <param name="maxDistance">The maximum distance the sensor can detect.</param>
        public void Update(Transform droneTransform, float maxDistance)
        {
            // Calculate the world-space direction for the raycast
            Vector3 worldDirection = droneTransform.rotation * _direction;
            Vector3 origin = droneTransform.position;

            // Reset values before casting
            Reset(maxDistance);

            if (Physics.Raycast(origin, worldDirection, out RaycastHit hit, maxDistance))
            {
                // If the ray hits something, update properties
                DidHit = true;
                Distance = hit.distance;
                HitPoint = hit.point;

                Debug.Log($"Sensor hit: {hit.collider.name} at distance {Distance} from {origin} in direction {worldDirection}.");

                // Check the type of object hit
                TypeOfObstacle typeOfObstacle = hit.collider.GetComponent<TypeOfObstacle>();
                if (typeOfObstacle != null)
                {
                    IsObstacle = typeOfObstacle.IsObstacle;
                    IsForClean = typeOfObstacle.IsForClean;
                    IsMovable = typeOfObstacle.IsMovable;
                    IsNotForClean = typeOfObstacle.IsNotForClean;
                }
            }
        }

        /// <summary>
        /// Resets the sensor's readings to their default state (no hit).
        /// </summary>
        private void Reset(float maxDistance)
        {
            Distance = maxDistance;
            IsObstacle = 0;
            IsForClean = 0;
            IsMovable = 0;
            IsNotForClean = 0;
            DidHit = false;
            HitPoint = Vector3.zero;
        }

        /// <summary>
        /// Draws a debug gizmo in the editor to visualize the sensor's ray.
        /// </summary>
        public void DrawGizmo(Transform droneTransform, float maxDistance)
        {
            Vector3 worldDirection = droneTransform.rotation * _direction;
            Vector3 origin = droneTransform.position;

            if (DidHit)
            {
                // Draw a red line to the hit point
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, HitPoint);
                Gizmos.DrawSphere(HitPoint, 0.1f);
            }
            else
            {
                // Draw a green line to the max distance
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, origin + worldDirection * maxDistance);
            }
        }
    }
}
