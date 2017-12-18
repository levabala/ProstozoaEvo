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
        public delegate void NewZoaHandler(object sender, Protozoa zoa);
        public event NewZoaHandler OnNewZoa;

        public delegate void DeathZoaHandler(object sender, Protozoa zoa);
        public event DeathZoaHandler OnDeathZoa;

        public List<Protozoa> Protozoas = new List<Protozoa>();
        public List<Food> food = new List<Food>();

        long increment = 0;
        Random rnd = new Random();

        public double maxMoveSpeed = 100; //pixels/second
        public int maxFoodCount = 300;
        public double simSpeed = 1;
        public double aggressiveness = 0.1; // -radius/second (for radius^2)
        public double toxicity = 0.1; // intoxicication/second
        public double viscosity = 1.0001;// breaking/second
        public double fertility = 0.0005;// food_spawned*km^2/second
        public double foodWeight = 15;
        public double minLifeRadius = 1;
        public double noizeWeight = 0;
        public double interactCooldown = 2;
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
        public double rightLifeBorder = 1500;
        public double bottomLifeBorder = 1000;
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

        public void addZoa(Protozoa zoa)
        {
            zoa.id = increment++;
            lock (Protozoas)
                Protozoas.Add(zoa);

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
                FoodTick(worldTickTime);
                //ControlTick(time);
                calcMoving(worldTickTime);

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
            double o = optimalRadius;
            double k = 0.05;

            double res = k * Math.Pow((radius - o), 2) + 1;
            return res;
        }

        double notContolledTime = 0;
        private void calcMoving(double time)
        {
            notContolledTime += time;

            if (notContolledTime > controlTickTime)
                ControlTick(notContolledTime);

            lock (Protozoas)
            {
                List<Protozoa> toKill = new List<Protozoa>();
                Parallel.ForEach(Protozoas, (zoa) =>
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
                foreach (Protozoa zoa in toKill)
                    Protozoas.Remove(zoa);

                if (stablePopulationSize > Protozoas.Count)
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
                bufferedTime = time;
                return;
            }

            bufferedTime = 0;

            lock (food)
            {
                if (food.Count >= maxFoodCount)
                    return;
    
                int count = (int)(totalFertility * time);                
                for (int i = 0; i < count; i++)
                {
                    Pnt point = new Pnt(rnd.Next((int)leftLifeBorder, (int)rightLifeBorder), rnd.Next((int)topLifeBorder, (int)bottomLifeBorder));
                    food.Add(new Food(point, rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()));
                }
            }
        }

        public void ControlTick(double time)
        {
            notContolledTime = 0;
            
            lock (Protozoas)
            {
                List<Protozoa> newZoas = new List<Protozoa>();
                //Parallel.ForEach(Protozoas, (zoa) =>
                foreach (Protozoa zoa in Protozoas)
                {
                    //check cooldown
                    zoa.cooldown -= time;

                    //eat under-body food
                    if (zoa.cooldown <= 0)
                        lock (food)
                        {
                            foreach (Food f in food)
                            {
                                double dist = Vector.GetLength(zoa.centerP, f.point);
                                if (dist < zoa.radius)
                                {
                                    food.Remove(f);
                                    zoa.radius += foodWeight * zoa.eatValue(f);
                                    zoa.makeBusy(eatCooldown);
                                    break;
                                }
                            }
                        }

                    //change acceleration due to food distribution
                    double 
                        meatLeft = 0, meatRight = 0,
                        herbLeft = 0, herbRight = 0,
                        waterLeft = 0, waterRight = 0;
                    lock (food)
                        foreach (Food f in food)
                        {
                            double dist = Vector.GetLength(zoa.centerP, f.point);
                            if (dist > zoa.viewDepth * zoa.radius)
                                continue;
                            if (pointInTriangle(f.point, zoa.leftP, zoa.farCenterP, zoa.centerP))
                            {
                                meatLeft += f.meat / (dist * dist);
                                herbLeft += f.herb / (dist * dist);
                                waterLeft += f.water / (dist * dist);
                            }
                            else
                            if (pointInTriangle(f.point, zoa.rightP, zoa.farCenterP, zoa.centerP))
                            {
                                meatLeft += f.meat / (dist * dist);
                                herbLeft += f.herb / (dist * dist);
                                waterLeft += f.water / (dist * dist);
                            }
                        }
                    double zoasLeft = 0, zoasRight = 0, colorLeft = 0, colorRight = 0;
                    foreach (Protozoa z in Protozoas)
                        if (z.id != zoa.id)
                            if (pointInTriangle(z.centerP, zoa.leftP, zoa.farCenterP, zoa.centerP))
                            {
                                double dist = Vector.GetLength(zoa.centerP, z.centerP);
                                double coeff = 1 / (dist * dist);
                                if (dist == 0)
                                    coeff = 0;
                                zoasLeft += coeff * z.radius;
                                colorLeft = (colorLeft + coeff * ((zoa.color - z.color) / ZoaHSL.scale) * z.radius) / 2;
                            }
                            else
                            if (pointInTriangle(z.centerP, zoa.rightP, zoa.farCenterP, zoa.centerP))
                            {
                                double dist = Vector.GetLength(zoa.centerP, z.centerP);
                                double coeff = 1 / (dist * dist);
                                if (dist == 0)
                                    coeff = 0;
                                zoasRight += coeff * z.radius;
                                colorRight = (colorRight + coeff * ((zoa.color - z.color) / ZoaHSL.scale) * z.radius) / 2;
                            }

                    double sum = meatLeft + meatRight + herbLeft + herbRight + waterLeft + waterRight;
                    double sumLeft = meatLeft + herbLeft + waterLeft;
                    double sumRight = meatRight + herbRight + waterRight;
                    double sumMeat = meatLeft + meatRight;
                    double sumHerb = herbLeft + herbRight;
                    double sumWater = waterLeft + waterRight;
                    double noFood = 0;
                    if (sum == 0)                    
                        sum = noFood = 1;
                    if (sumMeat == 0)
                        sumMeat = 1;
                    if (sumHerb == 0)
                        sumHerb = 1;
                    if (sumWater == 0)
                        sumWater = 1;

                    zoa.moveControl(new double[] {
                        sumLeft / sum,
                        sumRight / sum,
                        meatLeft / sumMeat,
                        meatRight / sumMeat,
                        herbLeft / sumHerb,
                        herbRight / sumHerb,
                        waterLeft / sumWater,
                        waterRight / sumWater,
                        noFood,                                                
                        colorLeft,
                        colorRight
                    }, time);

                    //now collision between zoas        
                    if (zoa.cooldown <= 0)
                        foreach (Protozoa zz in Protozoas)
                            if (zz.radius != 0 && zoa.radius > zz.radius && zz.id != zoa.id)
                            {
                                double dist = Vector.GetLength(zoa.centerP, zz.centerP);
                                if (dist < zz.radius + zoa.radius)
                                {
                                    double[] input = new double[] {
                                        Math.Abs(zoa.color - zz.color) / ZoaHSL.scale,
                                        optimalRadius / zoa.radius - 1 };
                                    double[] res = zoa.interactControl(input);
                                    double toEat = res[0];
                                    double toLove = res[1];

                                    if (toLove <= 0 && toEat <= 0)
                                        continue;

                                    if (toLove > toEat)
                                    {
                                        Protozoa child = zoa.love(rnd, zz);
                                        if (child != null)
                                        {
                                            newZoas.Add(child);
                                            child.makeBusy(interactCooldown);
                                            zoa.makeBusy(interactCooldown);
                                        }
                                    }
                                    else
                                    {
                                        zoa.radius += zoa.eatValue(zz);
                                        zz.radius = 0;
                                        zoa.makeBusy(eatCooldown);
                                    }                                
                                }
                            }

                    //try to gemmate the Zoa
                    if (gemmating && zoa.cooldown <= 0)
                    {
                        Protozoa newZoa = zoa.gemmate(rnd);
                        if (newZoa != null)
                        {
                            newZoas.Add(newZoa);
                            zoa.makeBusy(interactCooldown);
                        }
                    }
                }//;
                foreach (Protozoa zoa in newZoas)
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
            if (b1 != b2)
                return false;
            b3 = sign(pt, v3, v1) < 0;
            return b2 == b3;
        }

        public static Protozoa generateRandomZoa(Random rnd, int left, int top, int right, int bottom)
        {
            double x = rnd.Next(left, right);
            double y = rnd.Next(top, bottom);
            double radius = rnd.Next(7, 15);
            double accPower = rnd.Next(30000, 50000);
            double rotationPower = rnd.NextDouble() * 1.3;
            double color = rnd.NextDouble() * ZoaHSL.scale * 2 - ZoaHSL.scale;
            double viewDepth = rnd.Next(15, 50);
            double viewWidth = rnd.Next(20, 40);
            double moveAngle = (double)rnd.Next(-314, 314) / 100;
            double moveLength = rnd.Next(3, 8);
            double digestibilityMeat = rnd.NextDouble();
            double digestibilityHerb = rnd.NextDouble();
            double digestibilityWater = rnd.NextDouble();
            Vector moveVector = new Vector(moveAngle, moveLength);
            Protozoa zoa = new Protozoa(rnd, x, y, new Constructor(radius, color, viewDepth, viewWidth, accPower, rotationPower, digestibilityMeat, digestibilityHerb, digestibilityWater));
            zoa.moveVector = moveVector;
            
            return zoa;
        }

        public static Protozoa generateRandomZoa(Random rnd, double left, double top, double right, double bottom)
        {
            return generateRandomZoa(rnd, (int)left, (int)top, (int)right, (int)bottom);            
        }

        
    }
}
