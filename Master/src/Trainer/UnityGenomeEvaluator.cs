using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
	public class UnityGenomeEvaluator : IGenomeListEvaluator<NeatGenome>
	{
		private ulong _evalCount;
		private UnityCommunication _unityCommunicator;
		public UnityGenomeEvaluator()
		{
			_evalCount = 0;
			_unityCommunicator = new UnityCommunication(Math.Min(1, Environment.ProcessorCount-2));
		}

		public ulong EvaluationCount => _evalCount;

		public bool StopConditionSatisfied => throw new NotImplementedException();

		public void Evaluate(IList<NeatGenome> genomeList)
		{
			
			_evalCount++;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}
