using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DroneBehaviour
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class DroneActions : MonoBehaviour
    {
        [Header("Ustawienia Ruchu")]
        [Tooltip("Prędkość poruszania się drona w przód/tył/boki/góra/dół.")]
        public float moveSpeed = 5f;

        [Tooltip("Prędkość obrotu drona w stopniach na sekundę.")]
        public float rotationSpeed = 90f;

        private Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        public void MakeAction(DroneMove action)
        {
            Debug.Log($"Wykonuję akcję: {action}");
            switch (action)
            {
                case DroneMove.Forward:
                    MoveForward();
                    break;
                case DroneMove.Backward:
                    MoveBackward();
                    break;
                case DroneMove.Leftward:
                    MoveLeft();
                    break;
                case DroneMove.Rightward:
                    MoveRight();
                    break;
                case DroneMove.Upward:
                    MoveUp();
                    break;
                case DroneMove.Downward:
                    MoveDown();
                    break;
                case DroneMove.RotateLeftward:
                    RotateLeft();
                    break;
                case DroneMove.RotateRightward:
                    RotateRight();
                    break;
                default:
                    StopMovement();
                    break;
            }
        }

        public void MoveForward()
        {
            rb.linearVelocity = transform.forward * moveSpeed;
        }

        public void MoveBackward()
        {
            rb.linearVelocity = -transform.forward * moveSpeed;
        }

        public void MoveLeft()
        {
            rb.linearVelocity = -transform.right * moveSpeed;
        }

        public void MoveRight()
        {
            rb.linearVelocity = transform.right * moveSpeed;
        }

        public void MoveUp()
        {
            rb.linearVelocity = transform.up * moveSpeed;
        }

        public void MoveDown()
        {
            rb.linearVelocity = -transform.up * moveSpeed;
        }

        public void RotateLeft()
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        public void RotateRight()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Zatrzymuje wszelki ruch drona. Należy wywoływać przed podjęciem nowej decyzji o ruchu.
        /// </summary>
        public void StopMovement()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
