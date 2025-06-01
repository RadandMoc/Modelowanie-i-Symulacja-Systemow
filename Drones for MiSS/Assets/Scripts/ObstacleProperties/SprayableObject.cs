using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class SprayableObject : MonoBehaviour
    {
        [SerializeField]
        private int sprayForFullCleaned = 1;

        [SerializeField]
        private double weight;

        private int overallSpray = 0;

        
        public void ChangeSpray(int spray)
        {
            overallSpray += spray;
        }

        public bool IsCleaned()
        {
            if (overallSpray >= sprayForFullCleaned)
            {
                return true;
            }
            return false;
        }


        public double CalculateSprayResult() 
        {
            double sprayResult = weight * Math.Min(overallSpray, sprayForFullCleaned) / sprayForFullCleaned; 
            return sprayResult;
        }
    }
}
