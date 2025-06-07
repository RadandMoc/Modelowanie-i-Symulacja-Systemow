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

        public TraditionalFitnessCalculate(MultiCollisionBehaviour collisionDetector)
        {
            this.collisionDetector = collisionDetector;
			Debug.Log("XD");
        }

        public double Evaluate()
        {
            double collision = collisionDetector.CalculateAllCollisionTime();
            return Math.Max(0.0, 100 + -Math.Max(collision, collision * collision) + sprayableObjects.Sum(x => x.CalculateSprayResult()) + notSprayableObjects.Sum(x => x.calculateSprayResult()));
        }
    }
}
