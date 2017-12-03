using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelObjective
{
    public class Net
    {
        public static int INPUT_COUNT = 4;
        public static int OUTPUT_COUNT = 1;
        public static int NEURONS_COUNT = 4;
        public static double MUTATION_LIMIT = 1;
        public static double CONNECTION_MAX_WEIGHT = 10;

        public double[] input, neurons, output;
        public double[,] conns1, conns2;

        public Net(Random rnd)
            : this(rnd, INPUT_COUNT, OUTPUT_COUNT, NEURONS_COUNT)
        {

        }

        public Net(Random rnd, int inputCount, int outputCount, int neuronsCount)
        {
            input = new double[inputCount];
            output = new double[outputCount];
            neurons = new double[neuronsCount];
            conns1 = new double[inputCount, neuronsCount];
            conns2 = new double[outputCount, neuronsCount];

            for (int i = 0; i < neurons.Length; i++)
                neurons[i] = rnd.NextDouble() * 2 - 1;            

            for (int i = 0; i < conns1.GetLength(0); i++)
                for (int i2 = 0; i2 < conns1.GetLength(1); i2++)                
                    conns1[i, i2] = rnd.NextDouble() * CONNECTION_MAX_WEIGHT * 2 - CONNECTION_MAX_WEIGHT;

            for (int i = 0; i < conns2.GetLength(0); i++)
                for (int i2 = 0; i2 < conns2.GetLength(1); i2++)
                    conns2[i, i2] = rnd.NextDouble() * CONNECTION_MAX_WEIGHT * 2 - CONNECTION_MAX_WEIGHT;
        }

        public Net(double[] input, double[,] conns1, double[] neurons, double[,] conns2, double[] output)
        {
            this.input = input;
            this.output = output;
            this.conns1 = conns1;
            this.conns2 = conns2;
            this.neurons = neurons;
        }

        public Net(Random rnd, Net net1, Net net2, double coeff1, double coeff2, double mutationRate)
        {
            input = net1.input;
            output = net1.output;
            neurons = new double[net1.neurons.Length];
            conns1 = new double[net1.conns1.GetLength(0), net1.conns1.GetLength(1)];
            conns2 = new double[net1.conns2.GetLength(0), net1.conns2.GetLength(1)];

            for (int i = 0; i < neurons.Length; i++)
            {
                double value = net1.neurons[i] * coeff1 + net2.neurons[i] * coeff2 + Genome.getMutation(rnd, MUTATION_LIMIT, mutationRate);
                if (value < -1)
                    value = -1;
                if (value > 1)
                    value = 1;
                neurons[i] = value;
            }
            for (int i = 0; i < conns1.GetLength(0); i++)
                for (int i2 = 0; i2 < conns1.GetLength(1); i2++)
                {
                    double value = net1.conns1[i, i2] * coeff1 + net2.conns1[i, i2] * coeff2 + Genome.getMutation(rnd, MUTATION_LIMIT, mutationRate);
                    if (value < -1)
                        value = -1;
                    if (value > 1)
                        value = 1;
                    conns1[i, i2] = value;
                }
            for (int i = 0; i < conns2.GetLength(0); i++)
                for (int i2 = 0; i2 < conns2.GetLength(1); i2++)
                {
                    double value = net1.conns2[i, i2] * coeff1 + net2.conns2[i, i2] * coeff2 + Genome.getMutation(rnd, MUTATION_LIMIT, mutationRate);
                    if (value < -1)
                        value = -1;
                    if (value > 1)
                        value = 1;
                    conns2[i, i2] = value;
                }
        }

        public Net(Random rnd, Net net, double mutationRate)
        {
            input = net.input;
            output = net.output;
            neurons = new double[net.neurons.Length];
            conns1 = new double[net.conns1.GetLength(0), net.conns1.GetLength(1)];
            conns2 = new double[net.conns2.GetLength(0), net.conns2.GetLength(1)];

            for (int i = 0; i < neurons.Length; i++)
                neurons[i] = net.neurons[i] + Genome.getMutation(rnd, MUTATION_LIMIT, mutationRate);
            for (int i = 0; i < conns1.GetLength(0); i++)
                for (int i2 = 0; i2 < conns1.GetLength(1); i2++)
                {
                    double value = net.conns1[i, i2] + Genome.getMutation(rnd, MUTATION_LIMIT, mutationRate);
                    if (value < -1)
                        value = -1;
                    if (value > 1)
                        value = 1;
                    conns1[i, i2] = value;
                }
            for (int i = 0; i < conns2.GetLength(0); i++)
                for (int i2 = 0; i2 < conns2.GetLength(1); i2++)
                {
                    double value = net.conns2[i, i2] + Genome.getMutation(rnd, MUTATION_LIMIT, mutationRate);
                    if (value < -1)
                        value = -1;
                    if (value > 1)
                        value = 1;
                    conns2[i, i2] = value;
                }
        }

        public double[] calc(double[] inputValues)
        {
            double[] neuronsBuf = new double[neurons.Length];
            double[] outputBuf = new double[output.Length];

            for (int i = 0; i < neuronsBuf.Length; i++)
                neuronsBuf[i] = 0;

            for (int i = 0; i < neurons.Length; i++)
            {
                for (int i2 = 0; i2 < input.Length; i2++)
                    neuronsBuf[i] += inputValues[i2] * conns1[i2, i];

                double alpha = neurons[i];
                double value = neuronsBuf[i];
                double exp = Math.Exp(alpha * value * 10);
                if (exp == -1)
                    continue;
                double processedValue = (exp - 1) / (exp + 1);

                for (int i3 = 0; i3 < outputBuf.Length; i3++)
                    outputBuf[i3] += processedValue;                
            }

            for (int i3 = 0; i3 < outputBuf.Length; i3++)
                if (outputBuf[i3] > 1)
                    outputBuf[i3] = 1;
                else
                if (outputBuf[i3] < -1)
                    outputBuf[i3] = -1;

            return outputBuf;
        }
    }
}
