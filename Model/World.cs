using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class World
    {
        public long counter = 0;
        public double simSpeed = 1;
        public double maxSpeed = 100;
        public double tickInterval = 0.001;
        public double maxMoveLength = 10; //pixels        

        Random rnd = new Random();
        public List<Protozoa> protozoas = new List<Protozoa>();
        public List<Food> food = new List<Food>();
        public Surface surface = new Surface();

        public World()
        {
            tickInterval = maxMoveLength / maxSpeed;
        }

        public void WorldTick(double time)
        {
            while (time > tickInterval)
            {
                FoodTick(tickInterval);
                ControlTick(tickInterval);
                MoveTick(tickInterval);
                time -= tickInterval;
            }
            FoodTick(time);
            ControlTick(time);
            MoveTick(time);
        }

        public double multipleTime(double time)
        {
            return simSpeed * time;
        }

        public void MoveTick(double time)
        {
            //let's move everybody
            lock (protozoas)
                foreach (Protozoa zoa in protozoas)
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
                foreach (Protozoa zoa in protozoas)
                {
                    if (killedZoas.Contains(zoa.id))
                        continue;                    

                    //control internal processes
                    double toxicity = surface.getEffectAtPoint(zoa.centerP, SourceType.Toxicity);
                    zoa.controlByViewField(protozoas, food, time);
                    zoa.controlEnergy(toxicity);

                    //go down cooldown
                    zoa.cooldown -= time;

                    if (zoa.cooldown > 0)
                        continue;

                    //control interacting
                    //with food                   
                    int eatFIndex = -1;
                    Parallel.For(0, food.Count, i =>
                    {                        
                        Food f = food[i];

                        InteractResult res = zoa.interactWithFood(f);
                        if (res == InteractResult.Eat)                                                    
                            eatFIndex = i;                                                    
                    });

                    if (eatFIndex != -1)
                        food.RemoveAt(eatFIndex);

                    //with other zoas  
                    continue;
                    foreach (Protozoa otherZoa in protozoas)
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
                    protozoas.RemoveAt(protozoas.FindIndex(z => { return z.id == id; }));
                foreach (Protozoa zoa in newZoas)
                    addZoa(zoa);
            }
        }

        long ff = 0;        
        public void FoodTick(double time)
        {
            double step = 5;
            foreach (SourcePoint spoint in surface.sourcePoints)
            {
                double dist = step;
                double rate = (1 / dist) * spoint.strength * time;
                double seed = rnd.NextDouble();
                bool toSpawn = seed < rate;
                while (dist < 500)
                {
                    if (seed < (1 / Math.Sqrt(dist)) * 0.001 * time)
                    {
                        double alpha = rnd.NextDouble() * Math.PI * 2;
                        Pnt foodPoint = Vector.GetEndPoint(spoint.location, alpha, dist);
                        double fire = surface.getEffectAtPoint(foodPoint, SourceType.Fire);
                        double grass = surface.getEffectAtPoint(foodPoint, SourceType.Grass);
                        double ocean = surface.getEffectAtPoint(foodPoint, SourceType.Ocean);
                        double toxicity = surface.getEffectAtPoint(foodPoint, SourceType.Toxicity);
                        Food f = new Food(foodPoint, fire, grass, ocean, toxicity);
                        food.Add(f);

                        Console.WriteLine(ff);
                        ff = 0;
                    }
                    else ff++;

                    seed = rnd.NextDouble();
                    dist += step;
                    rate = (1 / dist) * spoint.strength * time;                    
                }
            }
        }

        public void addZoa(Protozoa zoa)
        {
            zoa.id = counter;
            protozoas.Add(zoa);
            counter++;
        }

        public void addZoa(int distance)
        {
            addZoa(new Protozoa(rnd, surface.getRandomPoint(rnd, distance)));
        }        
    }
}
