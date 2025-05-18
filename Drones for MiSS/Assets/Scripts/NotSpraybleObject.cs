using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class NotSpraybleObject : MonoBehaviour
    {
        private int sprayed = 0;

        [SerializeField]
        private double weight = 1;

        public void ChangeSpray()
        {
            sprayed += 1;
        }

        public double calculateSprayResult()
        {
            return -weight * Math.Min(sprayed, 1);
        }

    }
}
