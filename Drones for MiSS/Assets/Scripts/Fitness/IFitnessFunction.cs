using log4net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IFitnessFunction
    {
        double Evaluate();

        void OnMoveMade(DroneMove move, UnityEngine.Transform trans);
    }
}
