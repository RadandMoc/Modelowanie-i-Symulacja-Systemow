using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField]
        private DroneSim drone;

        [SerializeField]
        private GameObject funcObject;

        private IFitnessFunction fitnessFunc;

        [SerializeField]
        private MultiCollisionBehaviour collisionBehaviour;

        [SerializeField]
        private Transform droneObj;


        public bool isFinished { get; private set; } = false;

        public uint GenomeId { get; private set; }

        private int ZAxisConst;

        private Vector3 normalizedVector;

        private double fitness = 0.0;

        private static readonly Vector3 CENTER = new Vector3(150, 0, 75);

        private static readonly Vector3 BOUNDARY = new Vector3(500, 0, 500);

        public void Initialize(FitnessType fitnessType, uint genomeId) 
        {
            switch (fitnessType)
            {
                case FitnessType.SimplyFitness:
                    fitnessFunc = funcObject.GetComponent<SimplyFitness>();
                    break;
                case FitnessType.TraditionalFitnessCalculate:
                    TraditionalFitnessCalculate traditionalFitness = funcObject.GetComponent<TraditionalFitnessCalculate>();
                    traditionalFitness.InitalizeZConst(ZAxisConst);
                    fitnessFunc = traditionalFitness;
                    break;
                default:
                    throw new ArgumentException($"Unknown fitness type: {fitnessType}");
            }
            GenomeId = genomeId;

        }

        public void InitializeDroneLogic(IBlackBox phenome, uint genomeId, int workerId, Vector3 vec, Quaternion rot, int zAxis)
        {

            DroneKinematics droneKin = droneObj.GetComponent<DroneKinematics>();
            droneKin.transform.position = vec;
            droneKin.transform.rotation = rot;
            droneKin.InitializeZAxis(zAxis);

            IGetInputs droneKinematics = droneKin;
            IGetInputs raycastHititng = droneObj.GetComponent<RaycastHitting>();

            drone.Initialize(phenome, new List<IGetInputs>() { droneKinematics, raycastHititng });
            ZAxisConst = zAxis;
            normalizedVector = new Vector3(0, 0, zAxis);
            Initialize(FitnessType.TraditionalFitnessCalculate, 0);
        }

        private bool DroneOutOfBounds()
        {
            Vector3 position = collisionBehaviour.transform.position;
            

            Vector3 leftBoundary = CENTER - BOUNDARY + normalizedVector;
            Vector3 rightBoundary = CENTER + BOUNDARY + normalizedVector;

            return position.x < leftBoundary.x || position.x > rightBoundary.x ||
                   position.z < leftBoundary.z || position.z > rightBoundary.z;

        }

        public void TriggerMove()
        {
            if (DroneOutOfBounds()) { 
                Debug.LogWarning("Drone is out of bounds. Ending simulation.");
                isFinished = true;
                fitness = fitnessFunc.AssessDroneFlewOutOfBounds(drone);
                transform.gameObject.SetActive(false);
                return; 
            }
            Debug.Log($"POZYCJA {collisionBehaviour.transform.position}");
            var move = drone.ClickKey();
            fitnessFunc.OnMoveMade(move, collisionBehaviour.transform);
        }

        public double ComputeFitness()
        {
            if (isFinished)
            {
                Debug.LogWarning("Fitness already computed. Returning previous value.");
                return this.fitness;
            }
            double fitness = fitnessFunc.Evaluate();
            Debug.Log($"Fitness computed: {fitness}");
            isFinished = true;
            return fitness;
        }
    }
}
