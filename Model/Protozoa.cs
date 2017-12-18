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
        public double            
            energy, energyCapacity, //energy can be used to: accelerate, increase radius, eat somebody, love
            radius,                 //decreases movement speed + increases intoxication
            fire,                   //red color
            grass,                  //green color
            ocean,                  //blue color
            weight,                 //just (radius * 2PI + energy) 
            intoxication,           //amount of toxicity into the body. increases mutation rate and radius loss
            cooldownEat,            //eat delay
            cooldownLove,           //love delay
            viewDepth,              //range of view
            viewWidth,              //width of view            
            fear,                   //danger memory
            fearfulness,            //fear active time
            accLimit;               //max possible acceleration
        public Vector
            moveVector,             //sum of accVector and breakingVector
            accVector;              //need energy to be increased
        public Genome 
            genome;                 //container for constructor and control nets

        public Protozoa(Random rnd, Pnt centerP, long id = 0)
        {
            this.centerP = centerP;
            this.id = id;
            genome = new Genome(rnd);
            applyConstructor(genome.constructor);
        }        

        public void move(double breakingRate, double time)
        {
            moveVector.add(accVector.multiply(time));
            moveVector.multiply(1 / breakingRate);            
            moveVector.next();
            centerP.x = moveVector.x1;
            centerP.y = moveVector.y1;
        }        

        public void controlAcc(List<Protozoa> zoas, List<Food> food)
        {
            calcViewEdges();

            List<double> foodAngles = new List<double>();
            List<double> foodFire = new List<double>();
            List<double> foodGrass = new List<double>();
            List<double> foodOcean = new List<double>();

            double fireFSum, grassFSum, oceanFSum;
            fireFSum = grassFSum = oceanFSum = 0;

            double fireF, grassF, oceanF, fizeZ, grassZ, oceanZ;
            fireF = grassF = oceanF = fizeZ = grassZ = oceanZ = 0;
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
                        double angle = Vector.GetAlpha(dx, dy) - moveVector.alpha;
                        fireFSum += f.fire;
                        grassFSum += f.grass;
                        oceanFSum += f.ocean;
                        foodAngles.Add(angle);                       
                        foodFire.Add(f.fire);
                        foodGrass.Add(f.grass);
                        foodOcean.Add(f.ocean);
                    }
                }

            for (int i = 0; i < foodAngles.Count; i++)
            {
                fireF += foodAngles[i] * (foodFire[i] / fireFSum);
                grassF += foodAngles[i] * (foodGrass[i] / grassFSum);
                oceanF += foodAngles[i] * (foodOcean[i] / oceanFSum);
            }

            foreach (Protozoa zoa in zoas)
            {
                double dist = Vector.GetLength(zoa.centerP, centerP);
                if (dist > viewDepth * radius || dist == 0)
                    continue;
                
                //TODO: add size to accNet + triangularing zoas
                if (pointInTriangle(zoa.centerP, leftViewP, rightViewP, centerP))
                {
                        
                    double coeff = 1 / (dist * dist);
                    if (dist == 0)
                        coeff = 0;
                    zoasLeft += coeff * z.radius;
                    colorLeft = (colorLeft + coeff * ((zoa.color - z.color) / ZoaHSL.scale) * z.radius) / 2;
                }                
            }

            double accAngle = genome.accAngleNet.calc();
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
}
