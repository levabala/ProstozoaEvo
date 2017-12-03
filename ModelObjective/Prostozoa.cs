using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelObjective
{
    public class Prostozoa
    {        
        public long id;

        public Pnt centerP, leftP, rightP, farCenterP;
        public double x, y, radius, accPower, color, viewDepth, viewWidth;
        public double interactCooldown = 1;
        public double cooldown = 0;
        public Vector moveVector, accVector;
        public Genome genome;

        double viewAngle;

        public Prostozoa(Random rnd, double x, double y, long id = 0)
        {
            this.x = x;
            this.y = y;
            this.id = id;

            moveVector = new Vector();
            accVector = new Vector();

            applyConstructor(rnd, new Constructor(rnd));
        }

        public Prostozoa(double x, double y, Genome genome, long id = 0)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.genome = genome;

            moveVector = new Vector();
            accVector = new Vector();

            applyConstructor(genome.constructor);            
        }

        public Prostozoa(
            Random rnd,
            double x, double y, Constructor constructor, long id = 0)
        {
            this.x = x;
            this.y = y;            
            this.id = id;

            moveVector = new Vector();
            accVector = new Vector();

            applyConstructor(rnd, constructor);                       
        }                      

        public Prostozoa gemmate(Random rnd)
        {
            double mutateRate = 0.1;
            if (radius < genome.constructor.radius * (1 + mutateRate) * 2)
                return null;

            moveVector.setLength(0);

            Genome childGenome = new Genome(rnd, genome, mutateRate, mutateRate);            

            Prostozoa newZoa = new Prostozoa(x + rnd.Next(-(int)radius, (int)radius), y + rnd.Next(-(int)radius, (int)radius), childGenome, 0);               
            radius -= newZoa.radius;

            makeBusy();

            return newZoa;
        }

        public void makeBusy()
        {
            cooldown = interactCooldown;
        }

        public void updatePoints()
        {
            centerP = new Pnt(x, y);
            farCenterP =
                new Vector(moveVector.alpha, viewDepth * radius)
                .setStart(x, y)
                .getEnd();
            leftP =
                new Vector(moveVector.alpha - Math.PI / 2, viewWidth / 2 * radius) //rotate to -90 degrees
                .setStart(farCenterP)
                .getEnd();
            rightP =
                new Vector(moveVector.alpha + Math.PI / 2, viewWidth / 2 * radius) //rotate to 90 degrees
                .setStart(farCenterP)
                .getEnd();
        }

        public void moveControl(double[] input)
        {
            double moveAngle = genome.moveNet.calc(input)[0] + moveVector.alpha;
            if (double.IsNaN(moveAngle) || double.IsInfinity(moveAngle))
                Console.WriteLine("WTF");
            accVector = new Vector(moveAngle, accPower);
        }

        private void applyConstructor(Constructor constructor)
        {
            radius = constructor.radius;
            color = constructor.color;
            accPower = constructor.accPower;
            viewDepth = constructor.viewDepth;
            viewWidth = constructor.viewWidth;            
            viewAngle = 2 * Math.Atan(viewWidth / viewDepth);            
        }

        private void applyConstructor(Random rnd, Constructor constructor)
        {
            applyConstructor(constructor);            
            genome = new Genome(rnd, constructor);            
        }
    }
}
