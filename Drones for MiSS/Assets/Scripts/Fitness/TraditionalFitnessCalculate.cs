using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private const double ACCEPTABLE_NORMALIZED_ENTROPY = 0.83;

        private const double ENTROPY_PENALTY_FACTOR = 80.0;

        private const double REWARD_FOR_CLOSE_DISTANCE_SPRAYABLE = 100.0;

        private const double CLOSE_DISTANCE_SPRAYABLE = 7.0f;

        private int closeToSpraybleCounter = 0;

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
            foreach (var sprayableObject in sprayableObjects)
            {
                if (Vector3.Distance( dronePos.position , sprayableObject.transform.position) < CLOSE_DISTANCE_SPRAYABLE)
                {
                    closeToSpraybleCounter++;
                    return;
                }
            }
        }

        private double RewardForCloseToSprayable() 
        {
            return closeToSpraybleCounter / turnCounter * REWARD_FOR_CLOSE_DISTANCE_SPRAYABLE;
        }

        public double Evaluate()
        {
            double collision = collisionDetector.CalculateAllCollisionTime();
            return Math.Max(0.0, 100 + -Math.Max(collision, collision * Math.Sqrt(collision) / 8) - PenaltyForPassivness() - CalculateEntropyPenalty() + RewardForCloseToSprayable() + sprayableObjects.Sum(x => x.CalculateSprayResult()) + notSprayableObjects.Sum(x => x.calculateSprayResult()));
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
