using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class TraditionalFitnessCalculate : MonoBehaviour, IFitnessFunction
    {
		[SerializeField]
        private MultiCollisionBehaviour collisionDetector;
		
        [SerializeField]
        private List<SprayableObject> sprayableObjects;

        [SerializeField]
        private List<NotSpraybleObject> notSprayableObjects;

        private const double ACCEPTABLE_NORMALIZED_ENTROPY = 0.8;

        private const double ENTROPY_PENALTY_FACTOR = 80.0;

        private const double REWARD_FOR_CLOSE_DISTANCE_SPRAYABLE = 100.0;

        private const double CLOSE_DISTANCE_SPRAYABLE = 50.0f;

        private double closeToSpraybleCounter = 0;

        private int turnCounter = 0;

        private HashSet<Vector3> visitedPositions;

        private Dictionary<DroneMove, int> moveCounter;



        private void Awake()
        {
            visitedPositions = new HashSet<Vector3>();
            moveCounter = new Dictionary<DroneMove, int>()
            {
                { DroneMove.Forward, 0 },
                { DroneMove.Backward, 0 },
                { DroneMove.Leftward, 0 },
                { DroneMove.Rightward, 0 },
                { DroneMove.Upward, 0 },
                { DroneMove.Downward, 0 },
                { DroneMove.RotateLeftward, 0 },
                { DroneMove.RotateRightward, 0 },
                { DroneMove.Spray, 0 },
                { DroneMove.DoNothing, 0},
            };
        }

        private const int MIN_VISITED_POSITIONS = 100;

        private const int PENALTY = 10;

        public TraditionalFitnessCalculate(MultiCollisionBehaviour collisionDetector)
        {
            this.collisionDetector = collisionDetector;
            visitedPositions = new HashSet<Vector3>();
            Debug.Log("XD");
        }

        /*
         * first fitness
        public double Evaluate()
        {
            double collision = collisionDetector.CalculateAllCollisionTime();
            return Math.Max(0.0, 100 + -Math.Max(collision, collision * collision) + sprayableObjects.Sum(x => x.CalculateSprayResult()) + notSprayableObjects.Sum(x => x.calculateSprayResult()));
        }
        */


        private double PenaltyForPassivness()
        {
            double penalty = 0.0;
            if (visitedPositions.Count < MIN_VISITED_POSITIONS)
            {
                penalty = PENALTY * (1 - visitedPositions.Count/MIN_VISITED_POSITIONS);
            }
            return penalty;
        }

        private double CalculateEntropyPenalty()
        {
            double totalMoves = moveCounter.Values.Sum();

            if (totalMoves == 0)
            {
                return 0.0;
            }

            double entropy = 0.0;
            int possibleMoveTypes = moveCounter.Count;

            foreach (var moveCount in moveCounter.Values)
            {
                if (moveCount > 0)
                {
                    double probability = moveCount / totalMoves;
                    entropy -= probability * Math.Log(probability, 2);
                }
            }

            double maxEntropy = Math.Log(possibleMoveTypes, 2);
            if (maxEntropy <= 0) return 0.0;

            double normalizedEntropy = entropy / maxEntropy;


            if (normalizedEntropy >= ACCEPTABLE_NORMALIZED_ENTROPY)
            {
                return 0.0;
            }
            else
            {
                
                double deficit = (ACCEPTABLE_NORMALIZED_ENTROPY - normalizedEntropy) / ACCEPTABLE_NORMALIZED_ENTROPY;
                return ENTROPY_PENALTY_FACTOR * deficit;
            }
        }

        private void UpdateSprayableCounter(Transform dronePos)
        {
            double reward = 0.0;
            float closest = float.MaxValue;

            foreach (var sprayableObject in sprayableObjects)
            {
                if (!sprayableObject.IsCleaned())
                {
                    float distance = Vector3.Distance(dronePos.position, sprayableObject.transform.position);
                    closest = Math.Min(closest, distance);
                }
                
            }

            if (closest < CLOSE_DISTANCE_SPRAYABLE)
            {
                const float minDistanceClamp = 5f;
                float clampedDistance = Math.Max(minDistanceClamp, closest);

                closeToSpraybleCounter += 1.0 / Math.Sqrt(clampedDistance);
            }
        }

        private double RewardForCloseToSprayable() 
        {
            return closeToSpraybleCounter / turnCounter * REWARD_FOR_CLOSE_DISTANCE_SPRAYABLE;
        }

        public double Evaluate()
        {
            double collision = collisionDetector.CalculateAllCollisionTime();
            return Math.Max(0.0, 100 + -Math.Max(collision, collision * Math.Sqrt(collision) / 6) - PenaltyForPassivness() - CalculateEntropyPenalty() + RewardForCloseToSprayable() + sprayableObjects.Sum(x => x.CalculateSprayResult()) + notSprayableObjects.Sum(x => x.calculateSprayResult()));
        }

        public void OnMoveMade(DroneMove move, Transform trans)
        {
            visitedPositions.Add(trans.position);
            moveCounter[move]++;
            UpdateSprayableCounter(trans);
            turnCounter++;


        }
    }
}
