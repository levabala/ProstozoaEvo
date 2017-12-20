using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct Food
    {
        public double fire, grass, ocean, fireRate, grassRate, oceanRate, toxicity;
        public Pnt point;

        public Food(Pnt point, double fire, double grass, double ocean, double toxicity)
        {
            this.point = point;
            this.toxicity = toxicity;
            this.fire = fire;
            this.grass = grass;
            this.ocean = ocean;
            double sum = fire + grass + ocean;
            fireRate = fire / sum;
            grassRate = grass / sum;
            oceanRate = ocean / sum;
        }
    }
}
