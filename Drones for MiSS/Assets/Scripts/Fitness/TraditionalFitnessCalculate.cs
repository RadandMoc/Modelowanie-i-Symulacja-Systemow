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

        private HashSet<Vector3> visitedPositions = new HashSet<Vector3>();

        private const int MIN_VISITED_POSITIONS = 100;

        private const int PENALTY = 10;

        public TraditionalFitnessCalculate(MultiCollisionBehaviour collisionDetector)
        {
            this.collisionDetector = collisionDetector;
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

        public double Evaluate()
        {
            double collision = collisionDetector.CalculateAllCollisionTime();
            return Math.Max(0.0, 100 + -Math.Max(collision, collision * collision) - PenaltyForPassivness() + sprayableObjects.Sum(x => x.CalculateSprayResult()) + notSprayableObjects.Sum(x => x.calculateSprayResult()));
        }

        public void OnMoveMade(DroneMove move, Transform trans)
        {
            visitedPositions.Add(trans.position);
        }
    }
}
