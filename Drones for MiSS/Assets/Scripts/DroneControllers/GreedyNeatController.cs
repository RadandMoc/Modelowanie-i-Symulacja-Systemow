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
			string neurons_val = "Wartosc z neuronu ";

			for (int i = 0; i < _outputs.Length; i++)
			{
				neurons_val += $"{i} -> {outputSignals[i]} ";
				if (outputSignals[i] > maxValue)
				{
					maxValue = outputSignals[i];
					bestIndex = i;
				}
			}
			//Debug.Log(neurons_val);
            return _outputs[bestIndex];
		}
	}
}
