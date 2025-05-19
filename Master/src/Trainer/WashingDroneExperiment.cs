using SharpNeat.Core;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Trainer
{
	internal class WashingDroneExperiment : IGuiNeatExperiment
	{
		private int _inputCount;
		private int _outputCount = 10;
		private int _defaultPopulationSize;
		private string _name = "Washing Drone Experiment";
		private string _description = "Washing Drone Experiment with NEAT algorithm";

		public WashingDroneExperiment(int inputCount) : this(inputCount, 6 * 5 + 9) { }
		public WashingDroneExperiment(int inputCount, int outputCount) : this(inputCount, outputCount, 20) { }
		public WashingDroneExperiment(int inputCount, int outputCount, int defaultPopulationSize)
		{
			_inputCount = inputCount;
			_outputCount = outputCount;
			_defaultPopulationSize = defaultPopulationSize;
		}

		public string Name => _name;

		public string Description => _description;

		public int InputCount => _inputCount;

		public int OutputCount => _outputCount;

		public int DefaultPopulationSize => _defaultPopulationSize;

		public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters => throw new NotImplementedException();

		public NeatGenomeParameters NeatGenomeParameters => throw new NotImplementedException();

		public AbstractDomainView CreateDomainView()
		{
			throw new NotImplementedException();
		}

		public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
		{
			throw new NotImplementedException();
		}

		public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize)
		{
			throw new NotImplementedException();
		}

		public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
		{
			throw new NotImplementedException();
		}

		public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
		{
			throw new NotImplementedException();
		}

		public IGenomeFactory<NeatGenome> CreateGenomeFactory()
		{
			throw new NotImplementedException();
		}

		public AbstractGenomeView CreateGenomeView()
		{
			throw new NotImplementedException();
		}

		public void Initialize(string name, XmlElement xmlConfig)
		{
			throw new NotImplementedException();
		}

		public List<NeatGenome> LoadPopulation(XmlReader xr)
		{
			throw new NotImplementedException();
		}

		public void SavePopulation(XmlWriter xw, IList<NeatGenome> genomeList)
		{
			throw new NotImplementedException();
		}
	}
}
