using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class Layer
    {
        public int pointsCount = 0;
        public int layerId = 0;
        public double joinDist = 0;
        public List<DinamicPointsSet> sets = new List<DinamicPointsSet>();
        public Layer(int layerId, double joinDist) { this.layerId = layerId; this.joinDist = joinDist; }

        public bool addPoint(DinamicPoint point)
        {
            pointsCount++;
            double minDx, minDy, minDist;
            int minId = -1;
            minDist = minDx = minDy = joinDist;
            for (int i = 0; i < sets.Count; i++)
            {
                DinamicPointsSet set = sets[i];
                if (set.type != point.type)
                    continue;
                double dx = point.x - set.x;
                double dy = point.y - set.y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist <= minDist)
                {
                    minId = i;
                    minDist = dist;
                    minDx = dx;
                    minDy = dy;                    
                }
            }
            if (minId != -1)
            {
                sets[minId].addPoint(point, minDx, minDy);
                return pointsCount % (layerId + 2) == 0;
            }
            DinamicPointsSet newSet = new DinamicPointsSet(point, joinDist);
            sets.Add(newSet);
            return pointsCount % (layerId + 2) == 0;
        }

        public bool addSet(DinamicPointsSet inSet)
        {
            pointsCount++;
            foreach (DinamicPointsSet set in sets)
            {
                if (set.type != inSet.type)
                    continue;
                double dx = inSet.x - set.x;
                double dy = inSet.y - set.y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist <= joinDist)
                {
                    set.addSet(inSet, dx, dy);
                    return pointsCount % (layerId + 2) == 0;
                }
            }
            sets.Add(inSet);
            return pointsCount % (layerId + 2) == 0;
        }
    }
}
