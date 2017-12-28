using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class DinamicPointsSet
    {
        public double x, y;
        public int type;
        public double joinDist;
        public List<DinamicPoint> points = new List<DinamicPoint>();
        public DinamicPointsSet(DinamicPoint point, double joinDist)
        {
            this.joinDist = joinDist;
            x = point.x;
            y = point.y;
            type = point.type;
            points.Add(point);
        }

        public void addPoint(DinamicPoint point)
        {
            double weight = points.Count;
            x += (point.x - x) / weight;
            y += (point.y - y) / weight;
            points.Add(point);
        }

        public void addSet(DinamicPointsSet set, double dx, double dy)
        {
            double w1 = points.Count;
            double w2 = set.points.Count;
            double coeff = w2 / (w2 + w1);
            x += coeff * dx;
            y += coeff * dy;
            points.AddRange(set.points);
        }

        public void addPoint(DinamicPoint point, double dx, double dy)
        {
            double weight = points.Count;
            x += dx / weight;
            y += dy / weight;
            points.Add(point);
        }
    }
}
