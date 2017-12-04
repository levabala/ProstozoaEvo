using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ModelObjective
{
    public class World 
    {
        public delegate void NewZoaHandler(object sender, Prostozoa zoa);
        public event NewZoaHandler OnNewZoa;

        public delegate void DeathZoaHandler(object sender, Prostozoa zoa);
        public event DeathZoaHandler OnDeathZoa;

        public List<Prostozoa> prostozoas = new List<Prostozoa>();
        public List<Pnt> food = new List<Pnt>();

        long increment = 0;
        Random rnd = new Random();

        public double maxMoveSpeed = 500;
        public int maxFoodCount = 3000;
        public double simSpeed = 1;
        public double aggressiveness = 0.05; // -radius/second (for radius^2)
        public double toxicity = 0.1; // intoxicication/second
        public double viscosity = 1.0001;// breaking/second
        public double fertility = 0.001;// food_spawned*km^2/second
        public double foodWeight = 3;
        public double minLifeRadius = 1;
        public double noizeWeight = 0.3;
        public double interactCooldown = 0.3;
        public double agressivenessRadiusWeight = 2;//1.02;
        public double optimalRadius = 6;
        public int stablePopulationSize = 5;

        public double SimSpeed { get { return simSpeed; } set { simSpeed = value; } }
        public double Aggressiveness { get { return aggressiveness; } set { aggressiveness = value; } }
        public double OptimalRadius { get { return optimalRadius; } set { optimalRadius = value; } }
        public double AgressivenessRadiusWeight { get { return agressivenessRadiusWeight; } set { agressivenessRadiusWeight = value; } }
        public double Toxicity { get { return toxicity; } set { toxicity = value; } }
        public double Viscosity { get { return viscosity; } set { viscosity = value; } }
        public double Fertility { get { return fertility; } set { fertility = value; } }
        public double FoodWeight { get { return foodWeight; } set { foodWeight = value; } }
        public double MaxMoveSpeed { get { return maxMoveSpeed; } set { maxMoveSpeed = value; } }
        public int MaxFoodCount { get { return maxFoodCount; } set { maxFoodCount = value; } }
        public double NoizeWeight { get { return noizeWeight; } set { noizeWeight = value; } }
        public int StablePopulationSize { get { return stablePopulationSize; } set { stablePopulationSize = value; } }
        public double LifeZoneWidth { get { return rightLifeBorder; } set { rightLifeBorder = value; } }
        public double LifeZoneHeight { get { return bottomLifeBorder; } set { bottomLifeBorder = value; } }

        public double leftLifeBorder = 0;
        public double rightLifeBorder = 800;
        public double bottomLifeBorder = 500;
        public double topLifeBorder = 0;
        public double area = 1;

        public double moveTickTime = 0.001; //in seconds
        public double controlTickTime = 0.001;        

        public World()
        {
            area = (rightLifeBorder - leftLifeBorder) * (bottomLifeBorder - topLifeBorder);
        }

        public void addZoa(Prostozoa zoa)
        {
            zoa.id = increment++;
            lock (prostozoas)
                prostozoas.Add(zoa);

            if (OnNewZoa != null)
                OnNewZoa(this, zoa);
        }

        public double multiplyTimeDueToSimSpeed(double time)
        {
            return time *= simSpeed;
        }

        public void MoveTick(double time)
        {
            if (time == 0)
                return;            

            while (time > moveTickTime)
            {
                calcMoving(time);
                time -= moveTickTime;
            }

            calcMoving(time);
        }

        double notContolledTime = 0;
        private void calcMoving(double time)
        {
            notContolledTime += time;

            if (notContolledTime > controlTickTime)
                ControlTick(notContolledTime);

            lock (prostozoas)
            {
                List<Prostozoa> toKill = new List<Prostozoa>();
                foreach (Prostozoa zoa in prostozoas)
                {
                    //apply aggressiveness
                    double coeff = aggressivenessFun(zoa.radius, agressivenessRadiusWeight, optimalRadius);
                    zoa.radius -= 
                        aggressiveness * 
                        coeff * 
                        time;

                    //kill to little zoas
                    if (zoa.radius < minLifeRadius)
                        toKill.Add(zoa);

                    //update coordinates
                    zoa.moveVector.setStart(zoa.x, zoa.y);
                    zoa.moveVector.add(zoa.accVector);
                    //zoa.moveVector.multiply(1 / (viscosity * time));
                    zoa.moveVector.multiply(time);
                    if (zoa.moveVector.length > maxMoveSpeed)
                        zoa.moveVector.setLength(maxMoveSpeed);

                    zoa.moveVector.next();
                    zoa.x += zoa.moveVector.dx * time;
                    zoa.y += zoa.moveVector.dy * time;

                    zoa.updatePoints();
                }
                foreach (Prostozoa zoa in toKill)
                    prostozoas.Remove(zoa);
                for (int i = 0; i < stablePopulationSize - prostozoas.Count; i++)
                    addZoa(generateRandomZoa(rnd, leftLifeBorder, topLifeBorder, rightLifeBorder, bottomLifeBorder));
            }
        }

        double bufferedTime = 0;
        public void FoodTick(double time)
        {
            double realTime = time;
            time += bufferedTime;

            double totalFertility = area * fertility / 1000;

            if (totalFertility * time < 1)
            {
                bufferedTime += realTime;
                return;
            }

            bufferedTime = 0;

            lock (food)
            {
                int count = (int)(totalFertility * time);
                for (int i = 0; i < count; i++)
                    food.Add(new Pnt(rnd.Next((int)leftLifeBorder, (int)rightLifeBorder), rnd.Next((int)topLifeBorder, (int)bottomLifeBorder)));

                if (food.Count > maxFoodCount)
                    food = food.Skip(food.Count - maxFoodCount).ToList();
            }
        }

        public void ControlTick(double time)
        {
            notContolledTime = 0;
            
            lock (prostozoas)
            {
                List<Prostozoa> newZoas = new List<Prostozoa>();
                foreach (Prostozoa zoa in prostozoas)
                {
                    //check cooldown
                    zoa.cooldown -= time;
                    if (zoa.cooldown > 0)
                        continue;

                    //eat under-body food
                    lock (food)
                    {
                        foreach (Pnt f in food)
                        {
                            double dist = new Vector(zoa.centerP, f).length;
                            if (dist < zoa.radius)
                            {
                                food.Remove(f);
                                zoa.radius += foodWeight;
                                zoa.makeBusy(interactCooldown);
                                break;
                            }
                        }
                    }

                    //change acceleration due to food distribution
                    double foodLeft = 0, foodRight = 0;
                    lock (food)
                        foreach (Pnt f in food)
                            if (pointInTriangle(f, zoa.leftP, zoa.farCenterP, zoa.centerP))
                                foodLeft += 1 / new Vector(zoa.centerP, f).length;
                            else
                            if (pointInTriangle(f, zoa.rightP, zoa.farCenterP, zoa.centerP))
                                foodLeft += 1 / new Vector(zoa.centerP, f).length;
                    double sum = foodLeft + foodRight;
                    double noFood = 0;
                    if (sum == 0)
                    {
                        sum = noFood = 1;
                    }
                    double noize = (rnd.NextDouble() * 2 - 1) * noizeWeight;
                    zoa.moveControl(new double[] { foodLeft / sum, foodRight / sum, noFood, noize }, time);

                    //try to gemmate the Zoa
                    Prostozoa newZoa = zoa.gemmate(rnd);
                    zoa.makeBusy(interactCooldown);
                    if (newZoa != null)
                        newZoas.Add(newZoa);
                }
                foreach (Prostozoa zoa in newZoas)
                    addZoa(zoa);
            }
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
            b3 = sign(pt, v3, v1) < 0;

            return ((b1 == b2) && (b2 == b3));
        }

        public static Prostozoa generateRandomZoa(Random rnd, int left, int top, int right, int bottom)
        {
            double x = rnd.Next(left, right);
            double y = rnd.Next(top, bottom);
            double radius = rnd.Next(5, 10);
            double accPower = rnd.Next(1000, 2000);
            double rotationPower = rnd.Next(5, 20);
            double color = rnd.Next(0, 255);
            double viewDepth = rnd.Next(55, 100);
            double viewWidth = rnd.Next(20, 40);
            double moveAngle = (double)rnd.Next(-314, 314) / 100;
            double moveLength = rnd.Next(3, 8);
            Vector moveVector = new Vector(moveAngle, moveLength);
            Prostozoa zoa = new Prostozoa(rnd, x, y, new Constructor(radius, color, viewDepth, viewWidth, accPower, rotationPower));
            zoa.moveVector = moveVector;
            
            return zoa;
        }

        public static Prostozoa generateRandomZoa(Random rnd, double left, double top, double right, double bottom)
        {
            return generateRandomZoa(rnd, (int)left, (int)top, (int)right, (int)bottom);            
        }

        private double aggressivenessFun(double radius, double alpha, double betta)
        {
            double a = -Math.Pow((radius - betta), 2);
            double b = Math.Pow(alpha, a);
            return -b + 1.5;
        }
    }
}
