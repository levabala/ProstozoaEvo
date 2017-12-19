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
            lock (controlLocker) {
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

                    //control interacting
                    //with food                   
                    int eatFIndex = -1;
                    Parallel.For(0, food.Count, i =>
                    {                        
                        Food f = food[i];

                        InteractResult res = zoa.interactWithFood(f, toxicity);
                        if (res == InteractResult.Eat)                                                    
                            eatFIndex = i;                                                    
                    });

                    if (eatFIndex != -1)
                        food.RemoveAt(eatFIndex);

                    //with other zoas
                    foreach (Protozoa otherZoa in protozoas)
                    {
                        if (otherZoa.radius > zoa.radius || killedZoas.Contains(zoa.id) || otherZoa.id == zoa.id)
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

        public void FoodTick(double time)
        {

        }
    }
}
