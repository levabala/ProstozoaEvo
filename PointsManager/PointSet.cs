using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xxHashSharp;

namespace PointsManager
{
    public class PointSet
    {
        public uint hash = 0; //(changes count)
        public Guid guid = Guid.NewGuid();
        public double x, y;
        public int type;
        public double joinDist;
        public List<ManagedPoint> points = new List<ManagedPoint>();
        public PointSet(ManagedPoint point, double joinDist) 
        {                                 
            this.joinDist = joinDist;
            x = point.x;
            y = point.y;
            type = point.type;
            hash = 0;
            points.Add(point);            
        }

        public void addPoint(ManagedPoint point)            
        {
            addPoint(point, point.x - x, point.y - y);            
        }

        public void addSet(PointSet set, double dx, double dy)
        {
            double w1 = points.Count;
            double w2 = set.points.Count;
            double coeff = w2 / (w2 + w1);
            x += coeff * dx;
            y += coeff * dy;
            points.AddRange(set.points);
        }

        public void addPoint(ManagedPoint point, double dx, double dy)
        {
            double weight = points.Count;
            x += dx / weight;
            y += dy / weight;
            points.Add(point);
            hash++;
        }

        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
