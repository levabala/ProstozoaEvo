using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class World
    {
        public List<Protozoa> protozoas = new List<Protozoa>();
        public List<Food> food = new List<Food>();
        public Surface surface = new Surface();

        public World() { }

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
                        if (otherZoa.radius < zoa.radius || killedZoas.Contains(zoa.id) || otherZoa.id == zoa.id)
                            continue;

                        InteractResult res = zoa.interactWithZoa(otherZoa, toxicity);
                        switch (res)
                        {
                            case InteractResult.Eat:

                        }                    
                    }
                }
            }
        }

        public void FoodTick(double time)
        {

        }
    }
}
