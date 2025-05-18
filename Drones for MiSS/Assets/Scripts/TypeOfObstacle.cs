using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class TypeOfObstacle : MonoBehaviour
    {
        [SerializeField]
        private double isObstacle;
        [SerializeField]
        private double isForClean;
        [SerializeField]
        private double isMovable;
        [SerializeField]
        private double isNotForClean;

        public double IsObstacle { get => isObstacle;}
        public double IsForClean { get => isForClean;}
        public double IsMovable { get => isMovable; }
        public double IsNotForClean { get => isNotForClean;}

        public void ChangeCleaning()
		{
			isForClean = 0.0f;
		}
    }
}
