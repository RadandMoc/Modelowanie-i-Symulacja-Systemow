using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
    public class NeatGenomeInitializer
    {
        public static NeatGenome GenerateNeat(NeatGenomeFactory factory)
        {
            int inputNumOfNeurons = factory.InputNeuronCount;
            int outputNumOfNeurons = factory.OutputNeuronCount;
            int hiddenNumOfNeurons = outputNumOfNeurons;
            uint genomeId = factory.NextGenomeId();

            var neuronGeneList = new NeuronGeneList(inputNumOfNeurons + outputNumOfNeurons);
            for (uint i = 0; i < inputNumOfNeurons; i++)
            {
                neuronGeneList.Add(new NeuronGene(i, NodeType.Input, 0));
            }
            for (uint i = (uint)inputNumOfNeurons; i < inputNumOfNeurons + hiddenNumOfNeurons; i++)
            {
                neuronGeneList.Add(new NeuronGene(i, NodeType.Hidden, 0));
            }
            for (uint i = (uint)(inputNumOfNeurons + hiddenNumOfNeurons);
                i < inputNumOfNeurons + hiddenNumOfNeurons + outputNumOfNeurons; i++)
            {
                neuronGeneList.Add(new NeuronGene(i, NodeType.Output, 0));
            }

            var connectionGeneList = new ConnectionGeneList(inputNumOfNeurons * hiddenNumOfNeurons + outputNumOfNeurons);
            Random random = new Random();
            for (uint i = 0; i < inputNumOfNeurons; i++)
            {
                for (uint j = (uint)inputNumOfNeurons; j < inputNumOfNeurons + hiddenNumOfNeurons; j++)
                {
                    uint connId = factory.InnovationIdGenerator.NextId;
                    connectionGeneList.Add(new ConnectionGene(connId, i, j, random.NextDouble()));
                }
            }
            if (hiddenNumOfNeurons == outputNumOfNeurons)
            {
                for (uint i = (uint)inputNumOfNeurons; i < inputNumOfNeurons + hiddenNumOfNeurons; i++)
                {
                    uint connId = factory.InnovationIdGenerator.NextId;
                    connectionGeneList.Add(new ConnectionGene(connId, i, i + (uint)hiddenNumOfNeurons, 1));
                }
            }
            else
                throw new NotImplementedException("Gamoniom nie chciało się robić dla różnej liczby neuronów ukrytych i wyjściowych");

            var genome = new NeatGenome(factory, genomeId, 0, neuronGeneList, connectionGeneList, factory.InputNeuronCount, factory.OutputNeuronCount, true);
            // Fabryka powinna zainicjalizować EvaluationInfo. Nie musimy nic więcej robić.
            return genome;
        }

    }
}
