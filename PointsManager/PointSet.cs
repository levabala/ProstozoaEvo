using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class PointSet<PointType> where PointType : StaticPoint
    {
        public double x, y;
        public int type;
        public double joinDist;
        public List<PointType> points = new List<PointType>();
        public PointSet(PointType point, double joinDist)
        {
            this.joinDist = joinDist;
            x = point.x;
            y = point.y;
            type = point.type;
            points.Add(point);
        }

        public void addPoint(PointType point)
        {
            double weight = points.Count;
            x += (point.x - x) / weight;
            y += (point.y - y) / weight;
            points.Add(point);
        }

        public void addSet(PointSet<PointType> set, double dx, double dy)
        {
            double w1 = points.Count;
            double w2 = set.points.Count;
            double coeff = w2 / (w2 + w1);
            x += coeff * dx;
            y += coeff * dy;
            points.AddRange(set.points);
        }

        public void addPoint(PointType point, double dx, double dy)
        {
            double weight = points.Count;
            x += dx / weight;
            y += dy / weight;
            points.Add(point);
        }
    }
}
