using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MathAssembly
{
    public struct Pnt
    {
        public double x, y;
        public Pnt(double x, double y)
        {
            this.x = x;
            this.y = y;            
        }

        public Point toPoint()
        {
            return new Point(x, y);
        }
    }
}
