using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xxHashSharp;

namespace BillionPointsManager
{
    public class PointSet
    {
        public Hashtable linkedObjects = new Hashtable();
        public uint hash = 0; //(changes count)
        public Guid guid = Guid.NewGuid();
        public double x, y, originX, originY;
        public int type;
        public double joinDist;
        public List<ManagedPoint> points = new List<ManagedPoint>();
        object locker = new object();

        public PointSet(ManagedPoint point, double joinDist) 
        {                                 
            this.joinDist = joinDist;
            originX = x = point.x;
            originY = y = point.y;
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
            lock (locker)
            {
                points.AddRange(set.points);                
                hash++;
            }
        }

        public void addPoint(ManagedPoint point, double dx, double dy)
        {
            double weight = points.Count;
            x += dx / weight;
            y += dy / weight;
            lock (locker)
            {
                points.Add(point);
            }
        }

        public void linkObject(Object key, Object obj)
        {
            linkedObjects.Add(key, obj);
        }
    }
}
