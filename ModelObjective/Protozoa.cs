using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelObjective
{
    public class Protozoa
    {        
        public long id;

        public Pnt centerP, leftP, rightP, farCenterP;
        public double 
            x, y, radius, accPower, rotationPower, color, 
            viewDepth, viewWidth,
            digestibilityMeat, digestibilityHerb, digestibilityWater;                
        public double cooldown = 0;
        public Vector moveVector, accVector;
        public Genome genome;

        double viewAngle;

        public Protozoa(Random rnd, double x, double y, long id = 0)
        {
            this.x = x;
            this.y = y;
            this.id = id;

            moveVector = new Vector();
            accVector = new Vector();

            applyConstructor(rnd, new Constructor(rnd));
        }

        public Protozoa(double x, double y, Genome genome, long id = 0)
        {
            this.x = x;
            this.y = y;
            this.id = id;
            this.genome = genome;

            moveVector = new Vector();
            accVector = new Vector();

            applyConstructor(genome.constructor);            
        }

        public Protozoa(
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

        public Protozoa gemmate(Random rnd)
        {
            double mutateRate = 0.1;
            if (radius < genome.constructor.radius * (1 + mutateRate) * 3)
                return null;

            moveVector.setLength(0);

            Genome childGenome = new Genome(rnd, genome, mutateRate, mutateRate);

            double dx = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            double dy = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            Protozoa newZoa = new Protozoa(x + dx, y + dy , childGenome, 0);               
            radius -= newZoa.radius;            

            return newZoa;
        }

        public Protozoa love(Random rnd, Protozoa zz)
        {
            double mutateRate = 0.1;

            Genome childGenome = new Genome(rnd, genome, zz.genome, radius, zz.radius, mutateRate, mutateRate);
            if (radius < childGenome.constructor.radius / 2 || zz.radius < childGenome.constructor.radius / 2)
                return null;

            moveVector.setLength(0);

            double dx = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            double dy = rnd.Next(-(int)radius, (int)radius) + radius * Math.Sign(rnd.Next(-1, 0));
            Protozoa newZoa = new Protozoa(x + dx, y + dy, childGenome, 0);
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
            acc = Math.Abs(acc);
            accVector = new Vector(moveAngle, accPower);// cc);
        }

        public double eatValue(Food f)
        {
            return f.meat * digestibilityMeat + f.herb * digestibilityHerb + f.water * digestibilityWater;
        }

        public double eatValue(Protozoa zoa)
        {
            double meat = zoa.radius * zoa.digestibilityMeat;
            double herb = zoa.radius * zoa.digestibilityHerb;
            double water = zoa.radius * zoa.digestibilityWater;
            return meat * digestibilityMeat + herb * digestibilityHerb + water * digestibilityWater;
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

            double sum = constructor.digestibilityMeat + constructor.digestibilityHerb + constructor.digestibilityWater;
            digestibilityMeat = constructor.digestibilityMeat / sum;
            digestibilityHerb = constructor.digestibilityHerb / sum;
            digestibilityWater = constructor.digestibilityWater / sum;
        }

        private void applyConstructor(Random rnd, Constructor constructor)
        {
            applyConstructor(constructor);            
            genome = new Genome(rnd, constructor);            
        }
    }
}
