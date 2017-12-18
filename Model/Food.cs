using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct Food
    {
        public double fire, grass, ocean;
        public Pnt point;

        public Food(Pnt point, double fire, double grass, double ocean)
        {
            this.point = point;
            this.fire = fire;
            this.grass = grass;
            this.ocean = ocean;
        }
    }
}
