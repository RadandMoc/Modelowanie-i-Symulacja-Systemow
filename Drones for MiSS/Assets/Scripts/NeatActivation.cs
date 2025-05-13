using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class NeatActivation
    {
        IBlackBox blackBox;
        private List<IGetInputs> inputSources;

        public NeatActivation(IBlackBox blackBox, List<IGetInputs> inputs)
        {
            this.blackBox = blackBox;
            this.inputSources = inputs;
        }


        private void ReadInputs() 
        {
            double[] inputs = inputSources.SelectMany(x => x.GetInputs()).ToArray();
            int inputsCount = inputSources.Sum(x => x.GetInputsCount());
            for (int i = 0; i < inputsCount; i++)
            {
                blackBox.InputSignalArray[i] = inputs[i];
            }
        }

        public double[] Activate() 
        {
            ReadInputs();
            blackBox.Activate();
            double[] outputs = new double[blackBox.OutputSignalArray.Length];
            for (int i = 0; i < blackBox.OutputSignalArray.Length; i++)
            {
                outputs[i] = blackBox.OutputSignalArray[i];
            }
            return outputs;
        }

    }
}
