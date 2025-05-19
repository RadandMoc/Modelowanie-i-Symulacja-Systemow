using SharpNeat.Core;
using SharpNeat.DistanceMetrics;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.SpeciationStrategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
	public class NeatManager
	{
		private const int _saveInterval = 1;
		private NeatEvolutionAlgorithm<NeatGenome> _ea;
		private IGenomeListEvaluator<NeatGenome> _genomeListEvaluator = new UnityGenomeEvaluator();
		private List<NeatGenome> _genomeList;
		private IActivationFunctionLibrary _activationFnLibrary = DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
		private IGenomeFactory<NeatGenome> _genomeFactory;
		// private readonly Stopwatch _stopwatch;
		// private readonly INeatExperiment _experiment;

		public NeatManager(int populationSize)
		{
			// _experiment = experiment;
			// _stopwatch = new Stopwatch();
			var eaParam = new NeatEvolutionAlgorithmParameters();
			eaParam.SpecieCount = 10;
			var complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Relative, 50);
			_ea = new NeatEvolutionAlgorithm<NeatGenome>(eaParam, new KMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric()), complexityRegulationStrategy);
			UpdateScheme updateScheme = new UpdateScheme(1);
			_ea.UpdateScheme = updateScheme;
			_ea.UpdateEvent += SaveToFile;
			_genomeFactory = new NeatGenomeFactory(6 * 5 + 9, 10, _activationFnLibrary);
			_genomeList = GenerateStartGenomes(populationSize);
		}
		public NeatManager(string fileName)
		{

		}

		private List<NeatGenome>? GenerateStartGenomes(int population)
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
			_ea.Initialize(_genomeListEvaluator, _genomeFactory, _genomeList);
			_ea.StartContinue();
		}

		private void SaveToFile(object sender, EventArgs e)
		{
			NetworkDefinition network = new NetworkDefinition(
				_ea.GenomeList[0].InputNodeCount,
				_ea.GenomeList[0].OutputNodeCount,
				_ea.)
			string fileName = $"genome_{_ea.CurrentGeneration}.xml";
			NetworkXmlIO.SaveComplete();
			GenomeXmlIO.SaveGenome(_genomeList[0], fileName);
		}
	}
}
