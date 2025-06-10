using log4net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class DronePositionGenerator
    {
        private List<List<Coords>> positions = new List<List<Coords>> {
            new List<Coords> { new Coords(134,3,19), new Coords(187, 10, 44), }, //z elewacjami szeroki
            new List<Coords> { new Coords(136,3,95), new Coords(187, 10, 120) }, // z oknami
            new List<Coords> { new Coords(206,3,55), new Coords(235, 10, 80) },
            new List<Coords> { new Coords(93,3,54), new Coords(115, 10, 80) }
        };
        private List<Coords> rotation = new List<Coords> { new Coords(0,0,0), new Coords(0,180,0), new Coords(0,-90,0), new Coords(0,90,0)};

        private double[] weights = new double[] { 0.4, 0.4, 0.1, 0.1 };

        int WeightedRandomIndex(Random rng)
        {
            double total = 0;
            foreach (var w in weights) total += w;

            double r = rng.NextDouble() * total;

            for (int i = 0; i < weights.Length; i++)
            {
                if (r < weights[i])
                    return i;
                r -= weights[i];
            }

            return weights.Length - 1;
        }

        public (UnityEngine.Vector3 vec, UnityEngine.Quaternion quat) GeneratePositionRotation(Random rng) 
        {
            int idx = WeightedRandomIndex(rng);
            double x = rng.NextDouble() * (positions[idx][1].x - positions[idx][0].x) + positions[idx][0].x;
            double y = rng.NextDouble() * (positions[idx][1].y - positions[idx][0].y) + positions[idx][0].y;
            double z = rng.NextDouble() * (positions[idx][1].z - positions[idx][0].z) + positions[idx][0].z;

            UnityEngine.Vector3 vec = new UnityEngine.Vector3((float)x, (float)y, (float)z);
            double rotX = rotation[idx].x;
            double rotY = rotation[idx].y;
            double rotZ = rotation[idx].z;
            UnityEngine.Quaternion quat = UnityEngine.Quaternion.Euler((float)rotX, (float)rotY, (float)rotZ);
            return (vec, quat);
        }

    }

    public struct Coords
    {
        public float x;
        public float y;
        public float z;
        public Coords(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }



}
