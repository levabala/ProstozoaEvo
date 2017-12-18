using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WorldCore
    {
        List<Protozoa> protozoas = new List<Protozoa>();
        List<Food> food = new List<Food>();        

        public void MoveTick(double time)
        {
            //let's move everybody
            lock (protozoas)
                foreach (Protozoa zoa in protozoas)
                {
                    moveZoa(zoa);
                }
        }

        private void moveZoa(Protozoa zoa)
        {

        }

        public void ControlTick(double time)
        {

        }

        public void FoodTick(double time)
        {

        }
    }
}
