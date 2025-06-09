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
            Random random = new Random((int)genomeId); // Użyj ID genomu jako ziarna dla powtarzalności

            var neuronGeneList = new NeuronGeneList(inputNumOfNeurons + outputNumOfNeurons + hiddenNumOfNeurons);
            // Dodawanie neuronów pozostaje bez zmian
            for (uint i = 0; i < inputNumOfNeurons; i++)
            {
                neuronGeneList.Add(new NeuronGene(i, NodeType.Input, 0));
            }
            uint hiddenStartIdx = (uint)inputNumOfNeurons;
            for (uint i = hiddenStartIdx; i < hiddenStartIdx + hiddenNumOfNeurons; i++)
            {
                neuronGeneList.Add(new NeuronGene(i, NodeType.Hidden, 0));
            }
            uint outputStartIdx = hiddenStartIdx + (uint)hiddenNumOfNeurons;
            for (uint i = outputStartIdx; i < outputStartIdx + outputNumOfNeurons; i++)
            {
                neuronGeneList.Add(new NeuronGene(i, NodeType.Output, 0));
            }

            var connectionGeneList = new ConnectionGeneList();

            // POPRAWIONA LOGIKA - Połączenia wejście -> warstwa ukryta
            for (uint i = 0; i < inputNumOfNeurons; i++)
            {
                for (uint j = hiddenStartIdx; j < hiddenStartIdx + hiddenNumOfNeurons; j++)
                {
                    connectionGeneList.Add(GetOrCreateConnectionGene(factory, i, j, random.NextDouble()));
                }
            }

            // POPRAWIONA LOGIKA - Połączenia warstwa ukryta -> wyjście
            if (hiddenNumOfNeurons == outputNumOfNeurons)
            {
                for (uint i = 0; i < hiddenNumOfNeurons; i++)
                {
                    connectionGeneList.Add(GetOrCreateConnectionGene(factory, hiddenStartIdx + i, outputStartIdx + i, 1.0));
                }
            }
            else
            {
                throw new NotImplementedException("Gamoniom nie chciało się robić dla różnej liczby neuronów ukrytych i wyjściowych");
            }

            return new NeatGenome(factory, genomeId, 0, neuronGeneList, connectionGeneList, factory.InputNeuronCount, factory.OutputNeuronCount, true);
        }

        /// <summary>
        /// Metoda pomocnicza, która sprawdza historię innowacji i tworzy lub pobiera istniejący gen.
        /// </summary>
        private static ConnectionGene GetOrCreateConnectionGene(NeatGenomeFactory factory, uint sourceId, uint targetId, double weight)
        {
            var connectionKey = new ConnectionEndpointsStruct(sourceId, targetId);
            uint innovationId; // Zmienna na ostateczny, poprawny numer innowacji
            uint? existingId;  // Zmienna nullable, której wymaga metoda TryGetValue

            // Spróbuj pobrać istniejący numer innowacji dla danego połączenia
            if (factory.AddedConnectionBuffer.TryGetValue(connectionKey, out existingId))
            {
                // SUKCES: Klucz został znaleziony.
                // Używamy istniejącego numeru innowacji. Musimy użyć .Value, aby uzyskać wartość z typu nullable.
                innovationId = existingId.Value;
            }
            else
            {
                // PORAŻKA: Klucz nie istnieje w historii.
                // Generujemy nowy numer innowacji i dodajemy go do bufora historii.
                innovationId = factory.NextInnovationId();
                factory.AddedConnectionBuffer.Enqueue(connectionKey, innovationId);
            }

            // Stwórz gen połączenia z poprawnym (nowym lub ponownie użytym) numerem innowacji
            return new ConnectionGene(innovationId, sourceId, targetId, weight);
        }

    }
}
