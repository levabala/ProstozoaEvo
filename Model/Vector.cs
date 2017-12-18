using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct Vector
    {
        public double x1, y1, x2, y2;
        public double dx, dy;
        public double length;
        public double alpha;

        public Vector(Pnt p1, Pnt p2)
            : this(p1.x, p1.y, p2.x, p2.y)
        {

        }

        public Vector(double x1, double y1, double x2, double y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;

            dx = x2 - x1;
            dy = y2 - y1;

            length = alpha = 0;

            length = calcLength();
            alpha = calcAlpha();
        }        

        public Vector(double alpha, double length)
        {
            x1 = y1 = x2 = y2 = dx = dy = 0;
            this.length = 0;
            this.alpha = alpha;            
            setLength(length);            
        }        

        public static Vector Generate(double x, double y, double dx, double dy)
        {
            return new Vector(x, y, x + dx, y + dy);
        }

        public Pnt getStart()
        {
            return new Pnt(x1, y1);
        }

        public Pnt getEnd()
        {
            return new Pnt(x2, y2);
        }

        public Vector multiply(double rate)
        {
            dx *= rate;
            dy *= rate;
            x2 = x1 + dx;
            y2 = y1 + dy;

            length = calcLength();

            return this;
        }

        public Vector next()
        {
            move(dx, dy);
            return this;
        }

        public Vector setStart(double x, double y)
        {
            return move(x - x1, y - y1);
        }

        public Vector setStart(Pnt point)
        {
            return move(point.x - x1, point.y - y1);
        }

        public Vector move(double dx, double dy)
        {
            x1 += dx;
            x2 += dx;
            y1 += dy;
            y2 += dy;

            return this;
        }

        public Vector setLength(double length)
        {
            this.length = length;
            x2 = length * Math.Cos(alpha) + x1;
            y2 = length * Math.Sin(alpha) + y1;
            dx = x2 - x1;
            dy = y2 - y1;

            return this;
        }

        public Vector add(Vector v)
        {
            add(v.dx, v.dy);

            return this;
        }

        public Vector add(double dx, double dy)
        {
            x2 += dx;
            y2 += dy;
            this.dx += dx;
            this.dy += dy;

            length = calcLength();
            alpha = calcAlpha();

            return this;
        }

        public double calcLength()
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public double calcAlpha()
        {            
            if (dx == 0)
            {
                alpha = 0;
                return 0;
            }
            double angle = Math.Atan(dy / dx);
            if ((dx < 0 && dy < 0) || (dx < 0 && dy >= 0))
                angle = angle - Math.PI;

            return angle;
        }

        public static double GetAlpha(double dx, double dy)
        {
            if (dx == 0)                            
                return 0;            
            double angle = Math.Atan(dy / dx);
            if ((dx < 0 && dy < 0) || (dx < 0 && dy >= 0))
                angle -= Math.PI;

            return angle;
        }

        public Vector clone()
        {
            Vector v = new Vector();
            v.x1 = x1;
            v.y1 = y1;
            v.x2 = x2;
            v.y2 = y2;
            v.alpha = alpha;
            v.length = length;
            v.dx = dx;
            v.dy = dy;

            return v;
        }        

        public static double GetLength(Pnt p1, Pnt p2)
        {
            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double GetLength(double dx, double dy)
        {            
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
