using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Protozoa
    {               
        public Pnt                  //shoutcuts to positions and view edges
            centerP,
            leftViewP,
            rightViewP,
            centerViewP;
        public long
            id;
        public bool
            fetalStage;             //if the zoa hasn't been already borned
        public Protozoa
            fetus = null;
        public double            
            energy, energyCapacity, //energy can be used to: accelerate, increase radius, eat somebody, love
            radius,                 //decreases movement speed + increases intoxication
            fire,                   //red color
            grass,                  //green color
            ocean,                  //blue color
            weight,                 //just (radius * radius * PI + energy) 
            intoxication,           //amount of toxicity into the body. increases mutation rate and radius loss
            cooldownEat,            //eat delay
            cooldownLove,           //love delay
            viewDepth,              //range of view
            viewWidth,              //width of view            
            fear,                   //danger memory
            fearfulness,            //fear active time
            color,                  //HSL hue (saturation = 1, luminosity = 0.5)    
            accLimit;               //max possible acceleration
        public Vector
            moveVector,             //sum of accVector and breakingVector
            accVector;              //need energy to be increased
        public Genome 
            genome;                 //container for constructor and control nets

        public Protozoa(Random rnd, Protozoa primaryParent, Protozoa secondaryParent, long id = 0)
        {
            this.id = id;
            centerP = primaryParent.centerP;
            genome = new Genome(rnd, primaryParent.genome, secondaryParent.genome);
            applyConstructor(genome.constructor);
        }

        public Protozoa(Random rnd, Pnt centerP, long id = 0)
        {
            this.centerP = centerP;
            this.id = id;
            genome = new Genome(rnd);
            applyConstructor(genome.constructor);
        }        

        public void move(double breakingRate, double time)
        {
            if (fetalStage)
                return;

            if (breakingRate < 1)
                breakingRate = 1;
            moveVector.add(accVector.multiply(time));
            moveVector.multiply(1 / breakingRate);            
            moveVector.next();
            centerP.x = moveVector.x1;
            centerP.y = moveVector.y1;

            if (fetus != null)
                fetus.centerP = centerP;
        }                

        public void controlByViewField(List<Protozoa> zoas, List<Food> food, double time)
        {
            calcViewEdges();

            Vector foodFire = Vector.ZeroVector;
            Vector foodGrass= Vector.ZeroVector;
            Vector foodOcean = Vector.ZeroVector;
            
            lock (food)
                foreach (Food f in food)
                {
                    double dx = f.point.x - centerP.x;
                    double dy = f.point.y - centerP.y;
                    double dist = Vector.GetLength(dx, dy);
                    if (dist > viewDepth * radius)
                        continue;
                    if (pointInTriangle(f.point, leftViewP, rightViewP, centerP))
                    {
                        dist -= radius;
                        double coeff = 1 / (dist * dist);
                        double angle = Vector.GetAlpha(dx, dy) - moveVector.alpha;
                        Vector v = new Vector(angle, f.fire * coeff);
                        foodFire.add(v);
                        v.setLength(f.grass * coeff);
                        foodGrass.add(v);
                        v.setLength(f.ocean * coeff);
                        foodOcean.add(v);
                    }
                }


            Vector zoaFire = Vector.ZeroVector;
            Vector zoaGrass = Vector.ZeroVector;
            Vector zoaOcean = Vector.ZeroVector;
            foreach (Protozoa zoa in zoas)
            {
                double dx = zoa.centerP.x - centerP.x;
                double dy = zoa.centerP.y - centerP.y;
                double dist = Vector.GetLength(dx, dy);
                if (dist > viewDepth * radius || dist == 0)
                    continue;
                                
                if (pointInTriangle(zoa.centerP, leftViewP, rightViewP, centerP))
                {
                    dist -= radius;
                    double coeff = 1 / (dist * dist);
                    double angle = Vector.GetAlpha(dx, dy) - moveVector.alpha;
                    Vector v = new Vector(angle, zoa.fire * radius * coeff);
                    zoaFire.add(v);
                    v.setLength(zoa.grass * radius * coeff);
                    zoaGrass.add(v);
                    v.setLength(zoa.ocean * radius * coeff);
                    zoaOcean.add(v);

                    double[] fearIn = new double[]
                    {
                        zoa.radius - radius,
                        Math.Abs(zoa.color - color)
                    };
                    double[] fearRes = genome.fearNet.calc(fearIn);
                    //when we see big men unlike you you'll be a little fearfull
                    fear += fearRes[0] * fearfulness * time;
                }                
            }
            //let the fear go down a bit
            fear -= (1 - fearfulness) * time;
            if (fear > 1)
                fear = 1;
            else if (fear < 0)
                fear = 0;
            
            double[] input = new double[]
            {
                foodFire.alpha / Math.PI,
                foodGrass.alpha / Math.PI,
                foodOcean.alpha / Math.PI,
                zoaFire.alpha / Math.PI,
                zoaGrass.alpha / Math.PI,
                zoaOcean.alpha / Math.PI,
                radius,
                energy,
                fear
            };
            double[] res = genome.accAngleNet.calc(input);
            double accAngle = res[0];

            accVector.alpha = accAngle;
        }

        public void controlEnergy(double toxicity)
        {
            double[] input = new double[]
            {
                energy,
                radius,
                intoxication,
                toxicity,
                fear
            };
            double[] res = genome.energyUseNet.calc(input);
            double speedUp = res[0];
            double radiusUp = res[1];
            double consumptionRate = res[2];

            if ((speedUp <= 0 && radiusUp <= 0) || consumptionRate <= 0)
                return;

            double sum = speedUp + radiusUp;
            double speedCoeff = speedUp / sum;
            double radiusCoeff = radiusUp / sum;
            double energyToUse = energy * consumptionRate;

            double speedUpEnergy = energyToUse * speedCoeff;
            double radiusUpEnergy = energyToUse * radiusCoeff;

            accelerate(speedUpEnergy);
            increaseRadius(radiusUpEnergy);
        }

        public InteractResult interactWithZoa(Protozoa zoa, double toxicity)
        {
            double[] input = new double[]
            {
                zoa.fire,
                zoa.grass,
                zoa.ocean,
                toxicity,
                fear,
                energy
            };
            double[] res = genome.interactZoaNet.calc(input);
            double toEat = res[0];
            double toLove = res[1];

            if (toEat < 0 && toLove < 0)
                return InteractResult.Nothing;
            if (toLove > toEat)            
                return InteractResult.Love;            

            eat(zoa.fire, zoa.grass, zoa.ocean, toxicity);
            return InteractResult.Eat;
        }

        public InteractResult interactWithFood(Food f, double toxicity)
        {
            double[] input = new double[]
            {
                f.fire,
                f.grass,
                f.ocean,
                toxicity,
                fear,
                energy
            };
            double[] res = genome.interactFoodNet.calc(input);
            bool toEat = (res[0] > 0) ? true : false;

            if (toEat)
            {
                eat(f.fire, f.grass, f.ocean, toxicity);
                return InteractResult.Eat;
            }
            return InteractResult.Nothing;
        }   

        public Protozoa love(Random rnd, Protozoa zoa)
        {
            Protozoa newZoa = new Protozoa(rnd, this, zoa);
            fetus = newZoa;
            return newZoa;
        }
        
        private void eat(double fire, double grass, double ocean, double toxicity)
        {
            radius += this.fire * fire + this.grass * grass + this.ocean * ocean;
            intoxication += toxicity;
        }

        private void accelerate(double energy)
        {
            if (energy > this.energy)
                energy = this.energy;

            weight = calcWeight(radius, this.energy);

            double acc = energy / weight;
            accVector.setLength(accVector.length + acc);

            this.energy -= energy;
        }

        private void increaseRadius(double energy)
        {
            if (energy > this.energy)
                energy = this.energy;

            double newRadius = Math.Sqrt((energy - Math.PI * radius * radius) / Math.PI);
            radius = newRadius;

            this.energy -= energy;
        }
        
        private static double calcWeight(double radius, double energy)
        {
            return radius * radius * Math.PI + energy;
        }

        private double sign(Pnt p1, Pnt p2, Pnt p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private bool pointInTriangle(Pnt pt, Pnt v1, Pnt v2, Pnt v3)
        {
            bool b1, b2, b3;
            b1 = sign(pt, v1, v2) < 0;
            b2 = sign(pt, v2, v3) < 0;
            if (b1 != b2)
                return false;
            b3 = sign(pt, v3, v1) < 0;
            return b2 == b3;
        }

        private void calcViewEdges()
        {
            centerViewP =
                new Vector(moveVector.alpha, viewDepth * radius)
                .setStart(centerP.x, centerP.y)
                .getEnd();
            leftViewP =
                new Vector(moveVector.alpha - Math.PI / 2, viewWidth / 2 * radius) //rotate to -90 degrees
                .setStart(centerViewP)
                .getEnd();
            rightViewP =
                new Vector(moveVector.alpha + Math.PI / 2, viewWidth / 2 * radius) //rotate to 90 degrees
                .setStart(centerViewP)
                .getEnd();
        }

        private void applyConstructor(Constructor constr)
        {
            energy = constr.getParamValue(ParamName.BirthEnergy);
            energyCapacity = constr.getParamValue(ParamName.EnergyCapacity);
            radius = constr.getParamValue(ParamName.BirthRadius);
            fire = constr.getParamValue(ParamName.Fire);
            grass = constr.getParamValue(ParamName.Grass);
            ocean = constr.getParamValue(ParamName.Ocean);
            cooldownEat = constr.getParamValue(ParamName.CooldownEat);
            cooldownLove = constr.getParamValue(ParamName.CooldownLove);
            viewDepth = constr.getParamValue(ParamName.ViewDepth);
            viewWidth = constr.getParamValue(ParamName.ViewWidth);
            fearfulness = constr.getParamValue(ParamName.Fearfulness);
            accLimit = constr.getParamValue(ParamName.AccLimit);

            fear = 0; //maybe we need to create individual part for Memory
        }
    }

    public enum InteractResult
    {
        Eat,
        Love,
        Nothing
    }
}
