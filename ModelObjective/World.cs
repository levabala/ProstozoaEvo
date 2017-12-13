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

        public double maxMoveSpeed = 50; //pixels/second
        public int maxFoodCount = 300;
        public double simSpeed = 1;
        public double aggressiveness = 0.1; // -radius/second (for radius^2)
        public double toxicity = 0.1; // intoxicication/second
        public double viscosity = 1.0001;// breaking/second
        public double fertility = 0.0005;// food_spawned*km^2/second
        public double foodWeight = 15;
        public double minLifeRadius = 1;
        public double noizeWeight = 0.3;
        public double interactCooldown = 0.2;
        public double eatCooldown = 0.2;
        public double agressivenessRadiusWeight = 2;//1.02;
        public double optimalRadius = 10;
        public int stablePopulationSize = 8;
        public bool gemmating = false;

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
        public double LifeZoneWidth { get { return rightLifeBorder; } set { rightLifeBorder = value; area = (rightLifeBorder - leftLifeBorder) * (bottomLifeBorder - topLifeBorder); } }
        public double LifeZoneHeight { get { return bottomLifeBorder; } set { bottomLifeBorder = value; area = (rightLifeBorder - leftLifeBorder) * (bottomLifeBorder - topLifeBorder); } }

        public double leftLifeBorder = 0;
        public double rightLifeBorder = 800;
        public double bottomLifeBorder = 500;
        public double topLifeBorder = 0;
        public double area = 1;

        public double worldTickTime = 0.05;
        public double moveTickTime = 0.05; //in seconds
        public double controlTickTime = 0.05;
        public double foodTickTime = 0.001;

        public World()
        {
            area = (rightLifeBorder - leftLifeBorder) * (bottomLifeBorder - topLifeBorder);

            Timer spawnTime = new Timer(1000);
            spawnTime.Elapsed += SpawnTime_Elapsed; ;
            //spawnTime.Start();
        }

        private void SpawnTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            addZoa(generateRandomZoa(rnd, leftLifeBorder, topLifeBorder, rightLifeBorder, bottomLifeBorder));
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

        public void WorldTick(double time)
        {
            if (time == 0)
                return;

            while(time > worldTickTime)
            {                
                FoodTick(time);
                //ControlTick(time);
                calcMoving(time);

                time -= worldTickTime;
            }

            FoodTick(time);            
            calcMoving(time);
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

        private double aggressivenessFun(double radius)
        {
            //https://www.desmos.com/calculator/flcikir4kw
            /*double b = 18;
            double a = 0.996;
            double c = 18.2;
            double o = optimalRadius;

            double res = b * (-Math.Pow(a, Math.Pow(radius - optimalRadius, 2))) + c;
            return res;*/

            double o = optimalRadius;
            double k = 0.01;

            double res = k * Math.Pow((radius - o), 2) + 1;
            return res;
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
                Parallel.ForEach(prostozoas, (zoa) =>
                {
                    //apply aggressiveness
                    double coeff = aggressivenessFun(zoa.radius);//, agressivenessRadiusWeight, optimalRadius);
                    zoa.radius -=
                        aggressiveness *
                        coeff *
                        time;

                    //kill to little zoas
                    if (zoa.radius < minLifeRadius)
                        lock(toKill)
                            toKill.Add(zoa);

                    //update coordinates
                    zoa.moveVector.setStart(zoa.x, zoa.y);
                    zoa.moveVector.add(zoa.accVector.clone().multiply(time));
                    //zoa.moveVector.multiply(1 / (viscosity * time));
                    zoa.moveVector.multiply(time);
                    zoa.moveVector.multiply(1 / zoa.radius);
                    if (zoa.moveVector.length > maxMoveSpeed * time)
                        zoa.moveVector.setLength(maxMoveSpeed * time);

                    zoa.moveVector.next();
                    zoa.x += zoa.moveVector.dx;
                    zoa.y += zoa.moveVector.dy;

                    zoa.updatePoints();
                });
                foreach (Prostozoa zoa in toKill)
                    prostozoas.Remove(zoa);

                if (stablePopulationSize > prostozoas.Count)
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
                //Parallel.ForEach(prostozoas, (zoa) =>
                foreach (Prostozoa zoa in prostozoas)
                {
                    //check cooldown
                    zoa.cooldown -= time;
                    if (zoa.cooldown > 0)
                        return;

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
                                zoa.makeBusy(eatCooldown);
                                break;
                            }
                        }
                    }

                    //change acceleration due to food distribution
                    double foodLeft = 0, foodRight = 0;
                    lock (food)
                        foreach (Pnt f in food)
                        {
                            double dist = new Vector(zoa.centerP, f).length;
                            if (dist > zoa.viewDepth * zoa.radius)
                                continue;
                            if (pointInTriangle(f, zoa.leftP, zoa.farCenterP, zoa.centerP))
                                foodLeft += 1 / dist;
                            else
                            if (pointInTriangle(f, zoa.rightP, zoa.farCenterP, zoa.centerP))
                                foodLeft += 1 / dist;
                        }
                    double zoasLeft = 0, zoasRight = 0, colorLeft = 0, colorRight = 0;
                    foreach (Prostozoa z in prostozoas)
                        if (z.id != zoa.id)
                            if (pointInTriangle(z.centerP, zoa.leftP, zoa.farCenterP, zoa.centerP))
                            {
                                double dist = new Vector(zoa.centerP, z.centerP).length;
                                double coeff = 1 / dist;
                                if (dist == 0)
                                    coeff = 0;
                                zoasLeft += coeff * z.radius;
                                colorLeft = (colorLeft + coeff * ((zoa.color - z.color) / ZoaHSL.scale) * z.radius) / 2;
                            }
                            else
                            if (pointInTriangle(z.centerP, zoa.rightP, zoa.farCenterP, zoa.centerP))
                            {
                                double dist = new Vector(zoa.centerP, z.centerP).length;
                                double coeff = 1 / dist;
                                if (dist == 0)
                                    coeff = 0;
                                zoasRight += coeff * z.radius;
                                colorRight = (colorRight + coeff * ((zoa.color - z.color) / ZoaHSL.scale) * z.radius) / 2;
                            }

                    double sum = foodLeft + foodRight;
                    double sum2 = zoasLeft + zoasRight;
                    double noFood = 0;
                    if (sum == 0)
                    {
                        sum = noFood = 1;
                    }
                    if (sum2 == 0)
                        sum2 = 1;

                    double noize = (rnd.NextDouble() * 2 - 1) * noizeWeight;

                    zoa.moveControl(new double[] {
                        foodLeft / sum,
                        foodRight / sum,
                        noFood,
                        noize,
                        zoasLeft / sum2,
                        zoasRight / sum2,
                        colorLeft,
                        colorRight
                    }, time);

                    //now collision between zoas                                   
                    foreach (Prostozoa zz in prostozoas)
                        if (zz.radius != 0 && zoa.radius > zz.radius && zz.id != zoa.id)
                        {
                            double dist = new Vector(zoa.centerP, zz.centerP).length;
                            if (dist < zz.radius + zoa.radius)
                            {
                                double[] input = new double[] { Math.Abs(zoa.color - zz.color) / ZoaHSL.scale, optimalRadius / zoa.radius - 1 };
                                double[] res = zoa.interactControl(input);
                                double toEat = res[0];
                                double toLove = res[1];

                                if (toLove <= 0 && toEat <= 0)
                                    continue;

                                if (toLove > toEat)
                                {
                                    Prostozoa child = zoa.love(rnd, zz);
                                    if (child != null)
                                    {
                                        newZoas.Add(child);
                                        zoa.makeBusy(interactCooldown);
                                    }
                                }
                                else
                                {
                                    zoa.radius += zz.radius;
                                    zz.radius = 0;
                                    zoa.makeBusy(eatCooldown);
                                }                                
                            }
                        }

                    //try to gemmate the Zoa
                    if (gemmating)
                    {
                        Prostozoa newZoa = zoa.gemmate(rnd);
                        if (newZoa != null)
                        {
                            newZoas.Add(newZoa);
                            zoa.makeBusy(interactCooldown);
                        }
                    }
                }//;
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
            double accPower = rnd.Next(10000, 20000);
            double rotationPower = rnd.NextDouble() * 1.3;
            double color = rnd.NextDouble() * ZoaHSL.scale * 2 - ZoaHSL.scale;
            double viewDepth = rnd.Next(15, 50);
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

        
    }
}
