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
        public double x, y, radius, accPower, rotationPower, color, viewDepth, viewWidth;                
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
            if (radius < genome.constructor.radius * (1 + mutateRate) * 3)
                return null;

            moveVector.setLength(0);

            Genome childGenome = new Genome(rnd, genome, mutateRate, mutateRate);

            double dx = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            double dy = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            Prostozoa newZoa = new Prostozoa(x + dx, y + dy , childGenome, 0);               
            radius -= newZoa.radius;            

            return newZoa;
        }

        public Prostozoa love(Random rnd, Prostozoa zz)
        {
            double mutateRate = 0.1;

            Genome childGenome = new Genome(rnd, genome, zz.genome, radius, zz.radius, mutateRate, mutateRate);
            if (radius < childGenome.constructor.radius / 2 || zz.radius < childGenome.constructor.radius / 2)
                return null;

            moveVector.setLength(0);

            double dx = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            double dy = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            Prostozoa newZoa = new Prostozoa(x + dx, y + dy, childGenome, 0);
            radius -= newZoa.radius / 2;
            zz.radius -= newZoa.radius / 2;

            return newZoa;
        }

        public void makeBusy(double cooldown)
        {
            this.cooldown = cooldown;
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

        public double[] interactControl(double[] input)
        {
            return genome.interactNet.calc(input);
        }

        public void moveControl(double[] input, double time)
        {
            double[] res = genome.moveNet.calc(input);
            double moveAngle = res[0] * time * rotationPower + moveVector.alpha;
            double acc = res[1] * accPower;
            if (double.IsNaN(moveAngle) || double.IsInfinity(moveAngle))
            {
                Console.WriteLine("WTF");
                return;
            }
            if (double.IsNaN(acc) || double.IsInfinity(acc))
            {
                Console.WriteLine("WTF");
                return;
            }
            if (Math.Abs(acc) < 0.1)
                Console.WriteLine("asd");
            acc = Math.Abs(acc);
            accVector = new Vector(moveAngle, accPower);// cc);
        }

        private void applyConstructor(Constructor constructor)
        {
            if (constructor.color > 5)
                constructor.color = 5;
            else if (constructor.color < 0)
                constructor.color = 0;

            radius = constructor.radius;
            color = constructor.color;
            accPower = constructor.accPower;
            viewDepth = constructor.viewDepth;
            viewWidth = constructor.viewWidth;
            rotationPower = constructor.rotationPower;
            viewAngle = 2 * Math.Atan(viewWidth / viewDepth);            
        }

        private void applyConstructor(Random rnd, Constructor constructor)
        {
            applyConstructor(constructor);            
            genome = new Genome(rnd, constructor);            
        }
    }
}
