using MathAssembly;
using PointsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class World
    {
        public object tickLocker = new object();        

        public long counter = 0;
        public double simSpeed = 1;
        public double maxSpeed = 100;
        public double moveInterval, foodInterval, controlInterval, minInterval;
        public double foodRate = 10;
        public double controlRate = 10;        
        public double maxMoveLength = 10; //pixels        

        Random rnd = new Random();
        public Dictionary<long, Protozoa> protozoas = new Dictionary<long, Protozoa>();
        public Dictionary<long, Food> food = new Dictionary<long, Food>();        
        public PointsManager.PointsManager pointsManager = new PointsManager.PointsManager(new Pnt(0, 0), 100);
        public Surface surface = new Surface();

        public World()
        {
            moveInterval = maxMoveLength / maxSpeed;
            foodInterval = moveInterval * foodRate;
            controlInterval = moveInterval * controlRate;
            minInterval = Math.Min(moveInterval, Math.Min(foodInterval, controlInterval));
        }

        public void WorldTick(double time)
        {
            lock (tickLocker)
            {
                double foodTime, controlTime, moveTime;
                foodTime = controlTime = moveTime = time;
                /*while (foodTime + controlTime + moveTime >= minInterval)
                {
                    if (foodTime > foodInterval)
                    {
                        FoodTick(foodInterval);
                        foodTime -= foodInterval;
                    }
                    if (controlTime > controlInterval)
                    {
                        ControlTick(controlInterval);
                        controlTime -= controlInterval;
                    }
                    if (moveTime > moveInterval)
                    {
                        MoveTick(moveInterval);
                        moveTime -= moveInterval;
                    }                                                
                }*/
                while (time > minInterval)
                {
                    FoodTick(minInterval);
                    ControlTick(minInterval);
                    MoveTick(minInterval);
                    time -= minInterval;
                }
                FoodTick(minInterval);
                ControlTick(minInterval);
                MoveTick(minInterval);
            }
        }

        public double multipleTime(double time)
        {
            return simSpeed * time;
        }

        public void MoveTick(double time)
        {
            //let's move everybody
            lock (protozoas)
                foreach (Protozoa zoa in protozoas.Values)
                    moveZoa(zoa, time);
        }

        private void moveZoa(Protozoa zoa, double time)
        {
            double viscosity = surface.getEffectAtPoint(zoa.centerP, SourceType.Viscosity);            
            zoa.move(viscosity, time);
        }

        object controlLocker = new object();
        public void ControlTick(double time)
        {
            lock (protozoas) {
                List<long> killedZoas = new List<long>();
                List<Protozoa> newZoas = new List<Protozoa>();
                foreach (Protozoa zoa in protozoas.Values)
                {
                    if (killedZoas.Contains(zoa.id))
                        continue;                    

                    //control internal processes
                    double toxicity = surface.getEffectAtPoint(zoa.centerP, SourceType.Toxicity);
                    DinamicPoint[] nearObjectsIds = pointsManager.getNeighbors(zoa.id);
                    List<Protozoa> nearZoas = new List<Protozoa>();
                    List<Food> nearFood = new List<Food>();
                    for (int i = 0; i < nearObjectsIds.Length; i++)
                    {
                        DinamicPoint point = nearObjectsIds[i];
                        switch (point.type)
                        {
                            case ZoaType:
                                nearZoas.Add(protozoas[point.id]);
                                break;
                            case FoodType:
                                nearFood.Add(food[point.id]);
                                break;
                        }                        
                    }

                    zoa.controlByViewField(nearZoas.ToArray(), nearFood.ToArray(), time);
                    zoa.controlEnergy(toxicity);

                    //go down cooldown
                    zoa.cooldown -= time;

                    if (zoa.cooldown > 0)
                        continue;

                    //control interacting
                    //with food                   
                    long eatFIndex = -1;
                    Parallel.ForEach(food, pair => 
                    {
                        Food f = pair.Value;
                        InteractResult res = zoa.interactWithFood(f);
                        if (res == InteractResult.Eat)                                                    
                            eatFIndex = f.id;                                                    
                    });

                    if (eatFIndex != -1)
                        food.Remove(eatFIndex);

                    //with other zoas  
                    continue;
                    foreach (Protozoa otherZoa in protozoas.Values)
                    {
                        double dist = Vector.GetLength(zoa.centerP, otherZoa.centerP);
                        if (dist > otherZoa.radius + zoa.radius || otherZoa.radius > zoa.radius || killedZoas.Contains(otherZoa.id) || otherZoa.id == zoa.id)
                            continue;
                        
                        InteractResult res = zoa.interactWithZoa(otherZoa, toxicity);
                        switch (res)
                        {
                            case InteractResult.Eat:
                                killedZoas.Add(otherZoa.id);
                                break;
                            case InteractResult.Love:                                
                                newZoas.Add(zoa.love(rnd, otherZoa));
                                break;
                        }                    
                    }
                }
                
                foreach (long id in killedZoas)
                    protozoas.Remove(id);
                foreach (Protozoa zoa in newZoas)
                    addZoa(zoa);
            }
        }

        long ff = 0;        
        public void FoodTick(double time)
        {
            lock (food)
            {
                double step = 10;
                foreach (SourcePoint spoint in surface.sourcePoints.Values)
                {
                    if (spoint.sourceType != SourceType.Fertility)
                        continue;

                    double dist = 10;
                    double rate = (1 / (Math.Sqrt(dist))) * spoint.strength * time;
                    double strenght = spoint.strength;
                    double seed = rnd.NextDouble();
                    bool toSpawn = seed < rate;
                    while (dist < spoint.distance)
                    {
                        if (seed < (1 / (Math.Sqrt(dist))) * strenght * time)
                        {
                            double alpha = rnd.NextDouble() * Math.PI * 2;
                            Pnt foodPoint = Vector.GetEndPoint(spoint.location, alpha, dist);
                            double fire = surface.getEffectAtPoint(foodPoint, SourceType.Fire);
                            double grass = surface.getEffectAtPoint(foodPoint, SourceType.Grass);
                            double ocean = surface.getEffectAtPoint(foodPoint, SourceType.Ocean);
                            double toxicity = surface.getEffectAtPoint(foodPoint, SourceType.Toxicity);
                            Food f = new Food(foodPoint, fire, grass, ocean, toxicity);
                            addFood(f);

                            ff = 0;
                            strenght /= 2;
                        }
                        else ff++;

                        seed = rnd.NextDouble();
                        dist += step;
                        rate = (1 / dist) * spoint.strength * time;
                    }
                }
            }
        }

        public void addZoa(Protozoa zoa)
        {
            zoa.id = counter;
            protozoas.Add(zoa.id, zoa);
            pointsManager.addPoint(zoa.centerP, zoa.viewDepth * zoa.radius, zoa.id, ZoaType);
            counter++;
        }
        
        public void addFood(Food f)
        {
            f.id = counter;
            food.Add(f.id, f);
            pointsManager.addStaticPoint(f.point, f.id, FoodType);
            counter++;
        }

        public void addZoa(int distance)
        {
            addZoa(new Protozoa(rnd, surface.getRandomPoint(rnd, distance)));
        }

        public void addSourcePoint(SourcePoint sourcePoint)
        {
            sourcePoint.id = counter;
            counter++;
            surface.addSourcePoint(sourcePoint);
            pointsManager.addStaticPoint(sourcePoint.location, sourcePoint.id, SourcePointType);
        }

        public const int ZoaType = 0;
        public const int FoodType = 1;
        public const int SourcePointType = 2;
    }
}
