using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class GreedyNeatController : IMakeAction
    {
        NeatActivation _neatActivation;
        private readonly DroneMove[] _outputs;

        public GreedyNeatController(NeatActivation neat, DroneMove[] outputs)
        {
            _neatActivation = neat;
            _outputs = outputs;
        }

        public DroneMove MakeAction() => GetBestAction(_neatActivation.Activate());

        private DroneMove GetBestAction(double[] outputSignals) 
        {
            double maxValue = double.MinValue;
            int bestIndex = -1;
            Debug.Log($"wyjsciowe {outputSignals.Length}");
            Debug.Log($"moje {_outputs.Length}");
            for (int i = 0; i < _outputs.Length; i++)
            {
                if (outputSignals[i] > maxValue)
                {
                    maxValue = outputSignals[i];
                    bestIndex = i;
                }
            }
            return _outputs[bestIndex];
        }


    }
}
