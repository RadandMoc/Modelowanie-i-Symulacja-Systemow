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

        public TraditionalFitnessCalculate(MultiCollisionBehaviour collisionDetector)
        {
            this.collisionDetector = collisionDetector;
        }

        public double Evaluate()
        {
            double collision = collisionDetector.CalculateAllCollisionTime();
            return -Math.Max(collision, collision * collision) + sprayableObjects.Sum(x => x.CalculateSprayResult());
        }
    }
}
