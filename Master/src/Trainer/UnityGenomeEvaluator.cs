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
		private Double _satisfyingFitness;
		private static int HARDWARE_THREADS = Math.Min(Math.Max(1, Environment.ProcessorCount - 1), 7);

		public UnityGenomeEvaluator()
		{
			_evalCount = 0;
			_unityCommunicator = new UnityCommunication(HARDWARE_THREADS);
		}
		public UnityGenomeEvaluator(ulong evaluationCount)
		{
			_evalCount = evaluationCount;
			_unityCommunicator = new UnityCommunication(HARDWARE_THREADS);
		}
		public UnityGenomeEvaluator(ulong evaluationCount, int threads)
		{
			_evalCount = evaluationCount;
			_unityCommunicator = new UnityCommunication(threads);
		}
		public UnityGenomeEvaluator(ulong evaluationCount, double wantedFitness)
		{
			_evalCount = evaluationCount;
			_unityCommunicator = new UnityCommunication(HARDWARE_THREADS);
			_satisfyingFitness = wantedFitness;
		}
		public UnityGenomeEvaluator(ulong evaluationCount, double wantedFitness, int threads)
		{
			_evalCount = evaluationCount;
			_unityCommunicator = new UnityCommunication(threads);
			_satisfyingFitness = wantedFitness;
		}


		public ulong EvaluationCount => _evalCount;

		public bool StopConditionSatisfied => 
			(Object.Equals(_satisfyingFitness,null)) ? false : _unityCommunicator.LastBestFitness >= _satisfyingFitness;

		public void Evaluate(IList<NeatGenome> genomeList)
		{
			_unityCommunicator.RunSimulations(genomeList);
			_evalCount++;
		}

		public void Reset() { }
	}
}
