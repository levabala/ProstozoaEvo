using MathAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Genome
    {
        public static double MUTATIVE = 0.1;

        //acc angle net (where to go)
        public Net accAngleNet = new Net(new int[] { 9, 9, 5, 3, 1 });
        /* 9 -> 9 -> 5 -> 3 -> 1
         * FireFood
         * GrassFood
         * OceanFood
         * FireZoas
         * GrassZoas
         * OceanZoas         
         * myRadius
         * myEnergy
         * fear
         * out: acceleration angle [-1, 1]
         */
        //energy use net (how to use energy: speed up or increase radius)
        public Net energyUseNet = new Net(new int[] { 4, 4, 3, 3 });
        /* 4 -> 4 -> 3 -> 3
         * energy
         * radius
         * intoxication
         * toxicity (from environment)
         * fear
         * out: speed up        [-1, 1]
         * out: increase radius [-1, 1]
         * out: consumptionRate [-1, 1] (if < 0 => 0)
         */
        //interactFood net
        public Net interactFoodNet = new Net(new int[] { 6, 6, 3, 1 });
        /* 6 -> 6 -> 3 -> 1
         * Fire
         * Grass
         * Ocean
         * toxicity 
         * fear
         * energy
         * out: eat? [-1, 1]
         */
        //interactZoa net
        public Net interactZoaNet = new Net(new int[] { 6, 6, 3, 2 });
        /* 6 -> 6 -> 3 -> 2
         * Fire
         * Grass
         * Ocean
         * toxicity 
         * fear
         * energy
         * out: eat?  [-1, 1]
         * out: love? [-1, 1] (if both are < 0 => nothing to do)
         */
        //fear net
        public Net fearNet = new Net(new int[] { 2, 2, 1 });
        /*
         * radiusDifference (mine - his)
         * colorDifference  Math.abs(mine - his)
         * out: fear [-1, 1]
         */ 

        public Constructor constructor;

        public Genome(Random rnd)
        {
            constructor = new Constructor(rnd);
            accAngleNet.fillWeights(rnd);
            energyUseNet.fillWeights(rnd);
            interactFoodNet.fillWeights(rnd);
            interactZoaNet.fillWeights(rnd);
            fearNet.fillWeights(rnd);
        }

        public Genome(Random rnd, Constructor constructor)
        {
            this.constructor = constructor;
            accAngleNet.fillWeights(rnd);
            energyUseNet.fillWeights(rnd);
            interactFoodNet.fillWeights(rnd);
            interactZoaNet.fillWeights(rnd);
            fearNet.fillWeights(rnd);
        }

        public Genome(Random rnd, Genome g1, Genome g2, double weight1 = 1, double weight2 = 1, double addMutativeC = 0, double addMutativeN = 0)
        {
            double sum = weight1 + weight2;
            double coeff1 = weight1 / sum;
            double coeff2 = weight2 / sum;

            constructor = new Constructor(rnd, g1.constructor, g2.constructor, coeff1, coeff2, addMutativeC);

            accAngleNet = new Net(rnd, g1.accAngleNet, g2.accAngleNet, coeff1, coeff2, addMutativeN);
            energyUseNet = new Net(rnd, g1.energyUseNet, g2.energyUseNet, coeff1, coeff2, addMutativeN);
            interactFoodNet = new Net(rnd, g1.interactFoodNet, g2.interactFoodNet, coeff1, coeff2, addMutativeN);
            interactZoaNet = new Net(rnd, g1.interactZoaNet, g2.interactZoaNet, coeff1, coeff2, addMutativeN);
            fearNet = new Net(rnd, g1.fearNet, g2.fearNet, coeff1, coeff2, addMutativeN);
        }
    }
}
