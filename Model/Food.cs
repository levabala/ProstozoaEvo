using MathAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct Food
    {
        public long id;
        public double fire, grass, ocean, fireRate, grassRate, oceanRate, toxicity, size;
        public Pnt point;

        public Food(Pnt point, double fire, double grass, double ocean, double toxicity, long id = 0, double size = 1)
        {
            this.id = id;
            this.point = point;
            this.toxicity = toxicity;
            this.fire = fire;
            this.grass = grass;
            this.ocean = ocean;
            this.size = size;
            double sum = fire + grass + ocean;
            fireRate = fire / sum;
            grassRate = grass / sum;
            oceanRate = ocean / sum;
        }
    }
}
