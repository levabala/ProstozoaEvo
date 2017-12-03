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

        public double maxMoveSpeed = 100;
        public int maxFoodCount = 300;
        public double simSpeed = 1;        
        public double aggressiveness = 0.001; // -radius/second (for radius^2)
        public double toxicity = 0.1; // intoxicication/second
        public double viscosity = 1.0001;// breaking/second
        public double fertility = 1;// food_spawned/second
        public double foodWeight = 300;
        public double minLifeRadius = 1;

        public double SimSpeed { get { return simSpeed; } set { simSpeed = value; } }
        public double Aggressiveness { get { return aggressiveness; } set { aggressiveness = value; } }
        public double Toxicity { get { return toxicity; } set { toxicity = value; } }
        public double Viscosity { get { return viscosity; } set { viscosity = value; } }
        public double Fertility { get { return fertility; } set { fertility = value; } }
        public double FoodWeight { get { return foodWeight; } set { foodWeight = value; } }
        public double MaxMoveSpeed { get { return maxMoveSpeed; } set { maxMoveSpeed = value; } }
        public int MaxFoodCount { get { return maxFoodCount; } set { maxFoodCount = value; } }

        public double leftLifeBorder = 0;
        public double rightLifeBorder = 500;
        public double bottomLifeBorder = 300;
        public double topLifeBorder = 0;

        public World()
        {
            
        }

        public void addZoa(Prostozoa zoa)
        {
            zoa.id = increment++;
            lock (prostozoas)
                prostozoas.Add(zoa);

            if (OnNewZoa != null)
                OnNewZoa(this, zoa);
        }        

        public void MoveTick(double time)
        {
            if (time == 0)
                return;

            time *= simSpeed;

            lock (prostozoas)
            {
                List<Prostozoa> toKill = new List<Prostozoa>();
                foreach (Prostozoa zoa in prostozoas)
                {
                    //apply aggressiveness
                    zoa.radius -= aggressiveness * zoa.radius;// * (2 * Math.PI * zoa.radius * zoa.radius);

                    //kill to little zoas
                    if (zoa.radius < minLifeRadius)
                        toKill.Add(zoa);

                    //update coordinates
                    zoa.moveVector.setStart(zoa.x, zoa.y);
                    zoa.moveVector.add(zoa.accVector);
                    zoa.moveVector.multiply(1 / (viscosity * time));
                    if (zoa.moveVector.length > maxMoveSpeed)
                        zoa.moveVector.setLength(maxMoveSpeed);

                    zoa.moveVector.next();
                    zoa.x += zoa.moveVector.dx * time;
                    zoa.y += zoa.moveVector.dy * time;

                    zoa.updatePoints();
                }
                foreach (Prostozoa zoa in toKill)
                    prostozoas.Remove(zoa);
            }
        }

        double bufferedTime = 0;
        public void FoodTick(double time)
        {
            double realTime = time;
            time += bufferedTime;
            time *= simSpeed;

            if (fertility * time < 1)
            {
                bufferedTime += realTime;
                return;
            }

            bufferedTime = 0;

            lock (food)
            {
                int count = (int)(fertility * time);                 
                for (int i = 0; i < count; i++)
                    food.Add(new Pnt(rnd.Next((int)leftLifeBorder, (int)rightLifeBorder), rnd.Next((int)topLifeBorder, (int)bottomLifeBorder)));

                if (food.Count > maxFoodCount)
                    food = food.Skip(food.Count - maxFoodCount).ToList();
            }
        }

        public void ControlTick(double time)
        {
            time *= simSpeed;
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
                                zoa.radius += foodWeight / (2 * Math.PI * zoa.radius * zoa.radius);
                                zoa.makeBusy();
                                break;
                            }
                        }
                    }

                    //change acceleration due to food distribution
                    double foodLeft = 0, foodRight = 0;
                    lock (food)
                        foreach (Pnt f in food)
                            if (pointInTriangle(f, zoa.leftP, zoa.farCenterP, zoa.centerP))
                                foodLeft++;
                            else
                            if (pointInTriangle(f, zoa.rightP, zoa.farCenterP, zoa.centerP))
                                foodRight++;
                    double sum = foodLeft + foodRight;
                    double noFood = 0;
                    if (sum == 0)
                    {
                        sum = noFood = 1;
                    }
                    double noize = rnd.NextDouble() * 2 - 1;
                    zoa.moveControl(new double[] { foodLeft / sum, foodRight / sum, noFood, noize });

                    //try to gemmate the Zoa
                    Prostozoa newZoa = zoa.gemmate(rnd);
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
    }
}
