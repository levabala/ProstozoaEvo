using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Genome
    {
        //acc angle net (where to go)
        public Net accAngleNet = new Net(new int[] { 8, 8, 4, 2, 1 });
        /* 8 -> 8 -> 4 -> 2 -> 1
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
        public Net energyUseNet = new Net(new int[] { 4, 4, 2, 2 });
        /* 4 -> 4 -> 2 -> 2
         * energy
         * radius
         * intoxication
         * toxicity (from environment)
         * fear
         * out: speed up        [-1, 1]
         * out: increase radius [-1, 1] (if both are < 0 => use nothing)
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

        public Constructor constructor;

        public Genome(Random rnd)
        {
            constructor = new Constructor(rnd);
            accAngleNet.fillWeights(rnd);
            energyUseNet.fillWeights(rnd);
            interactFoodNet.fillWeights(rnd);
            interactZoaNet.fillWeights(rnd);
        }

        public Genome(Random rnd, Constructor constructor)
        {
            this.constructor = constructor;
            accAngleNet.fillWeights(rnd);
            energyUseNet.fillWeights(rnd);
            interactFoodNet.fillWeights(rnd);
            interactZoaNet.fillWeights(rnd);
        }

        public Genome(Random rnd, Genome g1, Genome g2, double weight1, double weight2, double mutativeC, double mutativeN)
        {
            double sum = weight1 + weight2;
            double coeff1 = weight1 / sum;
            double coeff2 = weight2 / sum;

            constructor = new Constructor(rnd, g1.constructor, g2.constructor, coeff1, coeff2, mutativeC);

            accAngleNet = new Net(rnd, g1.accAngleNet, g2.accAngleNet, coeff1, coeff2, mutativeN);
            energyUseNet = new Net(rnd, g1.energyUseNet, g2.energyUseNet, coeff1, coeff2, mutativeN);
            interactFoodNet = new Net(rnd, g1.interactFoodNet, g2.interactFoodNet, coeff1, coeff2, mutativeN);
            interactZoaNet = new Net(rnd, g1.interactZoaNet, g2.interactZoaNet, coeff1, coeff2, mutativeN);
        }
    }
}
