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
using System.Xml;

namespace Trainer
{
	public class NeatManager
	{
		#region fields
		private const int _saveInterval = 1;
		private const string WORKER_PATH = "Best";
        private const string GENERATION = "Generation";
        private NeatEvolutionAlgorithm<NeatGenome> _ea;
		private IGenomeListEvaluator<NeatGenome> _genomeListEvaluator;
		private List<NeatGenome> _genomeList;
		private IActivationFunctionLibrary _activationFnLibrary = DefaultActivationFunctionLibrary.CreateLibraryNeat(new ReLU());
		private IGenomeFactory<NeatGenome> _genomeFactory;
		private NeatGenomeFactory _neatGenomeFactory;
		private uint _savePopulationInterval;
		private const double SELECTION_PROPORTION = 0.35;
		
		private static IComplexityRegulationStrategy COMPLEXITY_REGULATION_STRATEGY = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Relative, 70);
		private static bool WANT_STARTING_FULLY_CONNECTED = false;
		private const double OFFSPRING_SEXUAL_PROPORTION = 0.95;
        private const double ELITISM = 0.2;
        private const double OFFSPRING_ASEXUAL_PROPORTION = 0.15;

		private const int INPUT_SENSOR = 9;

		// private readonly Stopwatch _stopwatch;
		// private readonly INeatExperiment _experiment;
		#endregion fields

		public bool IsRunning => _ea.RunState == RunState.Running;

		#region constructors
		public NeatManager(int populationSize, uint savePopulationInterval = 3)
		{
			// _experiment = experiment;
			// _stopwatch = new Stopwatch();
			var eaParam = new NeatEvolutionAlgorithmParameters();
			eaParam.SpecieCount = (int)(populationSize*0.25);
			eaParam.SelectionProportion = SELECTION_PROPORTION;
			eaParam.OffspringSexualProportion = OFFSPRING_SEXUAL_PROPORTION;
			eaParam.OffspringAsexualProportion = OFFSPRING_ASEXUAL_PROPORTION;
            eaParam.ElitismProportion = ELITISM;
            eaParam.InterspeciesMatingProportion = 0.15;

			_ea = new NeatEvolutionAlgorithm<NeatGenome>(eaParam, new KMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric(1.0, 0.0, 15.0)), COMPLEXITY_REGULATION_STRATEGY);
            var complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Relative, 50);
			UpdateScheme updateScheme = new UpdateScheme(1);
			_ea.UpdateScheme = updateScheme;
			_ea.UpdateEvent += SaveToFile;
            _neatGenomeFactory = new NeatGenomeFactory(INPUT_SENSOR * 5 + 9, 10, _activationFnLibrary);
			_genomeFactory = _neatGenomeFactory;
            _genomeList = GenerateStartGenomes(populationSize, WANT_STARTING_FULLY_CONNECTED);
			InitializeNeatParameters(_neatGenomeFactory.NeatGenomeParameters);
            var genomeEvaluator = new UnityGenomeEvaluator(_neatGenomeFactory);
			_genomeListEvaluator = genomeEvaluator;
			//_genomeList = GenerateStartGenomes(populationSize);
			_savePopulationInterval = savePopulationInterval;
			genomeEvaluator.ClearWorkers();
		}

		public NeatManager(List<NeatGenome> genomeList, uint savePopulationInterval = 3)
		{
			var eaParam = new NeatEvolutionAlgorithmParameters();
			eaParam.SpecieCount =  (int)(genomeList.Count * 0.25);
            eaParam.SelectionProportion = SELECTION_PROPORTION;
			eaParam.OffspringSexualProportion = OFFSPRING_SEXUAL_PROPORTION;
			eaParam.OffspringAsexualProportion = OFFSPRING_ASEXUAL_PROPORTION;
			eaParam.ElitismProportion = ELITISM;
			_ea = new NeatEvolutionAlgorithm<NeatGenome>(eaParam, new KMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric()), COMPLEXITY_REGULATION_STRATEGY);
            eaParam.InterspeciesMatingProportion = 0.15;
            var complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Relative, 50);
			UpdateScheme updateScheme = new UpdateScheme(1);
			_ea.UpdateScheme = updateScheme;
			_ea.UpdateEvent += SaveToFile;
			_neatGenomeFactory = new NeatGenomeFactory(INPUT_SENSOR * 5 + 9, 10, _activationFnLibrary);
			_genomeFactory = _neatGenomeFactory;
            InitializeNeatParameters(_neatGenomeFactory.NeatGenomeParameters);

            var genomeEvaluator = new UnityGenomeEvaluator(_neatGenomeFactory);
			_genomeListEvaluator = genomeEvaluator;
			_genomeList = genomeList; //GenerateStartGenomes(genomeList.Count);
			_savePopulationInterval = savePopulationInterval;
			genomeEvaluator.ClearWorkers();
		}

		public NeatManager(string path, uint savePopulationInterval = 3)
		{
			_neatGenomeFactory = new NeatGenomeFactory(INPUT_SENSOR * 5 + 9, 10, _activationFnLibrary);
			_genomeFactory = _neatGenomeFactory;
			var genomes = LoadGenomes(path);
			var eaParam = new NeatEvolutionAlgorithmParameters();
            eaParam.SpecieCount = (int)(genomes.Count * 0.25);
            eaParam.SelectionProportion = SELECTION_PROPORTION;
			eaParam.OffspringSexualProportion = OFFSPRING_SEXUAL_PROPORTION;
			eaParam.OffspringAsexualProportion = OFFSPRING_ASEXUAL_PROPORTION;
            eaParam.ElitismProportion = ELITISM;
            eaParam.InterspeciesMatingProportion = 0.15;
            _ea = new NeatEvolutionAlgorithm<NeatGenome>(eaParam, new KMeansClusteringStrategy<NeatGenome>(new ManhattanDistanceMetric(1.0, 0.0, 15.0)), COMPLEXITY_REGULATION_STRATEGY);
           
            InitializeNeatParameters(_neatGenomeFactory.NeatGenomeParameters);
            var complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Relative, 50);
			UpdateScheme updateScheme = new UpdateScheme(1);
			_ea.UpdateScheme = updateScheme;
			_ea.UpdateEvent += SaveToFile;
			var genomeEvaluator = new UnityGenomeEvaluator(_neatGenomeFactory);
			_genomeListEvaluator = genomeEvaluator;
			_genomeList = genomes; 
			_savePopulationInterval = savePopulationInterval;
			genomeEvaluator.ClearWorkers();
		}
		#endregion constructors

		private void InitializeNeatParameters(NeatGenomeParameters neatGenomeParameters)
		{
            _neatGenomeFactory.NeatGenomeParameters.ConnectionWeightMutationProbability = 0.75;
            _neatGenomeFactory.NeatGenomeParameters.AddNodeMutationProbability = 0.2;
            _neatGenomeFactory.NeatGenomeParameters.DeleteConnectionMutationProbability = 0.07;

            _neatGenomeFactory.NeatGenomeParameters.AddConnectionMutationProbability = 0.3;
			_neatGenomeFactory.NeatGenomeParameters.DisjointExcessGenesRecombinedProbability = 0.85;
        }

        private List<NeatGenome> LoadGenomes(string path)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(path);
			return NeatGenomeXmlIO.LoadCompleteGenomeList(xmlDoc, true, _neatGenomeFactory);
		}

		private List<NeatGenome> GenerateStartGenomes(int population, bool fullyConnected = true)
		{
			if (fullyConnected)
			{
				List<NeatGenome> neatGenome = new List<NeatGenome>(population);
				for (int i = 0; i < population; i++)
					neatGenome.Add(NeatGenomeInitializer.GenerateNeat(_neatGenomeFactory));
				return neatGenome;
			}
			return [.. _neatGenomeFactory.CreateGenomeList(population, 0)]; // Używamy fabryki do wygenerowania genomów
		}

		public void Start()
		{
			_ea.Initialize(_genomeListEvaluator, _genomeFactory, _genomeList);
			
            _ea.StartContinue();
            
		}

		private void SaveToFile(object sender, EventArgs e)
		{
            if ( _ea.CurrentGeneration % _savePopulationInterval == 0)
            {
                string filePopulationPath = Path.Combine(GENERATION, $"population{_ea.CurrentGeneration}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}.xml");
                try
                {
                    using (XmlWriter xw = XmlWriter.Create(filePopulationPath, new XmlWriterSettings { Indent = true }))
                    {
                        NeatGenomeXmlIO.WriteComplete(xw, _ea.GenomeList, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas zapisu populacji {_ea.CurrentGeneration}");
                    // Rozważ logowanie błędu lub inną obsługę
                }
            }
			double genomeVal = _ea.GenomeList.Max<NeatGenome>(g => g.EvaluationInfo.Fitness);
			var genome = _ea.GenomeList.FirstOrDefault<NeatGenome>(g => g.EvaluationInfo.Fitness == genomeVal);
			//var genome = _ea.GenomeList[0]; // Zakładamy, że zapisujemy tylko pierwszy genom
			Directory.CreateDirectory(WORKER_PATH);

		
            string filePath = Path.Combine(WORKER_PATH, $"genome{_ea.CurrentGeneration}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}.xml");
            try
            {
                using (XmlWriter xw = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true }))
                {
                    // Zakładamy, że nodeFnIds=true jest odpowiednie dla Twojego przypadku (często wymagane)
                    // Jeśli nie potrzebujesz ID funkcji aktywacji dla węzłów, zmień na false.
                    NeatGenomeXmlIO.Write(xw, genome, true); // - Użycie metody Write z NeatGenomeXmlIO
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu genomu {genome.Id}: {ex.Message}");
                // Rozważ logowanie błędu lub inną obsługę
            }
        }

        /*
		 * public void SaveToFile(object sender, EventArgs e){
        NetworkDefinition network = new NetworkDefinition(
                _ea.GenomeList[0].InputNodeCount,
                _ea.GenomeList[0].OutputNodeCount,
                _activationFnLibrary, (NodeList)_ea.GenomeList[0].NodeList, _ea.GenomeList[0])

            string fileName = $"genome_{_ea.CurrentGeneration}.xml";
        NetworkXmlIO.SaveComplete();
			GenomeXmlIO.SaveGenome(_genomeList[0], fileName);}
		*/

    }
}
