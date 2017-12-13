using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelObjective
{
    public struct Genome
    {
        public static int moveIn = 8;
        public static int moveNeur = 4;
        public static int moveOut = 2;
        public static int intreractIn = 2;
        public static int intreractNeur = 2;
        public static int intreractOut = 2;
        public static double ALPHA_LIMIT = 1;

        public Constructor constructor;
        public Net moveNet, interactNet;

        public Genome(
            Random rnd,
            int paramsCount, int moveInput, int moveNeurons, int moveOutput, 
            int interactInput, int interactNeurons, int interactOutput)
        {
            constructor = new Constructor(rnd);            
            moveNet = new Net(rnd, moveInput, moveOutput, moveNeurons);
            interactNet = new Net(rnd, interactInput, interactOutput, interactNeurons);
        }

        public Genome(Random rnd, Constructor constructor)
        {
            this.constructor = constructor;
            moveNet = new Net(rnd, moveIn, moveOut, moveNeur);
            interactNet = new Net(rnd, intreractIn, intreractOut, intreractNeur);
        }

        public Genome(Constructor constructor, Net moveNet, Net interactNet)
        {
            this.constructor = constructor;
            this.moveNet = moveNet;
            this.interactNet = interactNet;
        }

        public Genome(Random rnd, Genome g1, Genome g2, double weight1, double weight2, double mutRateConstr, double mutRateNet)
        {            
            double sum = weight1 + weight2;
            double coeff1 = weight1 / sum;
            double coeff2 = weight2 / sum;

            constructor = new Constructor(rnd, g1.constructor, g2.constructor, coeff1, coeff2, mutRateConstr);

            moveNet = new Net(rnd, g1.moveNet, g2.moveNet, weight1, weight2, mutRateNet);
            interactNet = new Net(rnd, g1.interactNet, g2.interactNet, weight1, weight2, mutRateNet);            
        }  
        
        public Genome(Random rnd, Genome genome, double mutRateConstr, double mutRateNet)
        {
            constructor = genome.constructor.clone().mutate(rnd, mutRateConstr);
            moveNet = new Net(rnd, genome.moveNet, mutRateNet);
            interactNet = new Net(rnd, genome.interactNet, mutRateNet);
        }

        public static double getMutation(Random rnd, double mutationRate)
        {
            return rnd.NextDouble() * mutationRate * 2 - mutationRate;
        }
    }   
    
    public struct Constructor
    {
        public static int COUNT = 5;
        public double radius, color, viewDepth, viewWidth, accPower, rotationPower;

        public Constructor(Random rnd, Constructor constr1, Constructor constr2, double coeff1, double coeff2, double mutationRate)
        {
            radius = mutateIt(rnd, constr1.radius * coeff1 + constr2.radius * coeff2, mutationRate);
            color = mutateIt(rnd, constr1.color * coeff1 + constr2.color * coeff2, mutationRate);
            viewDepth = mutateIt(rnd, constr1.viewDepth * coeff1 + constr2.viewDepth * coeff2, mutationRate);
            viewWidth = mutateIt(rnd, constr1.viewWidth * coeff1 + constr2.viewWidth * coeff2, mutationRate);
            accPower = mutateIt(rnd, constr1.accPower * coeff1 + constr2.accPower * coeff2, mutationRate);
            rotationPower = mutateIt(rnd, constr1.rotationPower * coeff1 + constr2.rotationPower * coeff2, mutationRate);
        }

        public Constructor(Random rnd)
        {
            radius = rnd.NextDouble(); ;
            color = rnd.NextDouble();
            viewDepth = rnd.NextDouble();
            viewWidth = rnd.NextDouble();
            accPower = rnd.NextDouble();
            rotationPower = rnd.NextDouble();
        }

        public Constructor(double radius, double color, double viewDepth, double viewWidth, double accPower, double rotationPower)
        {
            this.radius = radius;
            this.color = color;
            this.viewDepth = viewDepth;
            this.viewWidth = viewWidth;
            this.accPower = accPower;
            this.rotationPower = rotationPower;
        }

        public Constructor clone()
        {
            return new Constructor(radius, color, viewDepth, viewWidth, accPower, rotationPower);
        }

        private static double mutateIt(Random rnd, double val1, double val2, double mutationRate)
        {
            double res = (val1 + val2) / 2 + (rnd.NextDouble() * 2 - 1) * mutationRate;
            /*if (res > 1)
                res = 1;
            else if (res < 0)
                res = 0;*/
            return res;
        }

        private static double mutateIt(Random rnd, double val, double mutationRate)
        {
            double res = val + (rnd.NextDouble() * 2 - 1) * mutationRate;
            /*if (res > 1)
                res = 1;
            else if (res < 0)
                res = 0;*/
            return res;
        }        

        public Constructor mutate(Random rnd, double mutationRate)
        {
            radius = mutateIt(rnd, radius, mutationRate);
            color = mutateIt(rnd, color, mutationRate);
            viewDepth = mutateIt(rnd, viewDepth, mutationRate);
            viewWidth = mutateIt(rnd, viewWidth, mutationRate);
            accPower = mutateIt(rnd, accPower, mutationRate);
            rotationPower = mutateIt(rnd, rotationPower, mutationRate);

            return this;
        }
    }
}
