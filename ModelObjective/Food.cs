using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelObjective
{
    public struct Food
    {
        public double meat, herb, water;
        public Pnt point;

        public Food(Pnt point, double meat, double herb, double water)
        {
            this.point = point;
            this.meat = meat;
            this.herb = herb;
            this.water = water;
        }
    }
}
