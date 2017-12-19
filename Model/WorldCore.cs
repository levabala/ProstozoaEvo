using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WorldCore
    {
        public List<Protozoa> protozoas = new List<Protozoa>();
        public List<Food> food = new List<Food>();
        public Surface surface = new Surface();

        public WorldCore() { }

        public void MoveTick(double time)
        {
            //let's move everybody
            lock (protozoas)
                foreach (Protozoa zoa in protozoas)                
                    moveZoa(zoa);                
        }

        private void moveZoa(Protozoa zoa)
        {
            double viscosity = getEffectAtPoint(zoa.centerP, SourceType.Viscosity);
        }

        public void ControlTick(double time)
        {

        }

        public void FoodTick(double time)
        {

        }
    }
}
