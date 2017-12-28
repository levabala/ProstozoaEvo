using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathAssembly
{
    public class Net
    {
        public static double MUTATIVE = 0.1;

        public double[][] neurons;
        public double[][,] conns;

        public Net(int[] layers)
        {            
            conns = new double[layers.Length - 1][,];
            neurons = new double[layers.Length - 2][];
            for (int i = 1; i < layers.Length - 1; i++)
            {
                conns[i-1] = new double[layers[i - 1], layers[i]];
                neurons[i - 1] = new double[layers[i]];
            }
            int last = conns.Length;
            conns[last - 1] = new double[layers[last - 1], layers[last]];
        }        

        public Net(Random rnd, Net net1, Net net2, double coeff1, double coeff2, double addMutative = 0)
        {
            addMutative += MUTATIVE;
            /*try
            {*/
                neurons = new double[net1.neurons.Length][];
                for (int i = 0; i < net1.neurons.Length; i++) {
                    neurons[i] = new double[net1.neurons[i].Length];
                    for (int i2 = 0; i2 < neurons[i].Length; i2++)
                    {                        
                        neurons[i][i2] =
                            net1.neurons[i][i2] * coeff1 +
                            net2.neurons[i][i2] * coeff2 +
                            rnd.NextDouble() * 2 * addMutative - addMutative;
                        if (neurons[i][i2] > 1)
                            neurons[i][i2] = 1;
                        else if (neurons[i][i2] < -1)
                            neurons[i][i2] = -1;
                    }
                }

                conns = new double[net1.conns.Length][,];
                for (int i = 0; i < net1.conns.Length; i++)
                {
                    conns[i] = new double[net1.conns[i].GetLength(0), net1.conns[i].GetLength(1)];
                    for (int i2 = 0; i2 < conns[i].GetLength(0); i2++)
                        for (int i3 = 0; i3 < conns[i].GetLength(1); i3++)
                        {
                            conns[i][i2, i3] =
                                net1.conns[i][i2, i3] * coeff1 +
                                net2.conns[i][i2, i3] * coeff2 +
                                rnd.NextDouble() * 2 * addMutative - addMutative;
                            
                        }
                }

                neurons = new double[net1.neurons.Length][];
                for (int i = 0; i < net1.neurons.Length; i++)
                {
                    neurons[i] = new double[net1.neurons[i].Length];
                    for (int i2 = 0; i2 < neurons[i].Length; i2++)                        
                        {
                            if (neurons[i][i2] > 1)
                                neurons[i][i2] = 1;
                            else if (neurons[i][i2] < -1)
                                neurons[i][i2] = 1;
                        }
                }
            
            /*}
            catch (Exception e)
            {
                throw new Exception("Not compatible nets");
            }*/
        }

        public void fillWeights(Random rnd)
        {            
            for (int i = 0; i < neurons.Length; i++)            
                for (int i2 = 0; i2 < neurons[i].Length; i2++)
                    neurons[i][i2] = rnd.NextDouble() * 2 - 1;

            for (int i = 0; i < conns.Length; i++)
                for (int i2 = 0; i2 < conns[i].GetLength(0); i2++)
                    for (int i3 = 0; i3 < conns[i].GetLength(1); i3++)
                        conns[i][i2, i3] = rnd.NextDouble() * 2 - 1;
        }

        public double[] calc(double[] inputData)
        {
            double[] nextInput = inputData;
            for (int i = 0; i < conns.Length - 1; i++)
                nextInput = calcLayer(nextInput, conns[i], neurons[i]);

            nextInput = calcLayer(nextInput, conns.Last());

            return nextInput; //now it's output
        }

        private double[] calcLayer(double[] input, double[,] connsTo)
        {
            double[] outputBuf = new double[connsTo.GetLength(1)];
            for (int i = 0; i < outputBuf.Length; i++)
                outputBuf[i] = 0;

            for (int i = 0; i < connsTo.GetLength(0); i++)            
                for (int o = 0; o < connsTo.GetLength(1); o++)
                {
                    double val = input[i] * connsTo[i, o];
                    outputBuf[o] += val;
                }            

            return outputBuf;
        }

        private double[] calcLayer(double[] input, double[,] connsTo, double[] neurons)
        {
            double[] outputBuf = calcLayer(input, connsTo);
            
            for (int i = 0; i < neurons.Length; i++)
            {
                double alpha = neurons[i]; //[-1, 1]
                double val = outputBuf[i];
                double exp = Math.Exp(alpha * val * 10);
                double processedValue = ((exp - 1) / (exp + 1) + 1) / 2; //[0, 1]
                val = processedValue;
                outputBuf[i] = val;
            }

            return outputBuf;
        }
    }
}
