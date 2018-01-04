using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class Layer
    {
        public int pointsCount = 0;
        public int setsCount = 0;
        public int layerId = 0;
        public double joinDist = 0;
        public PointSetsContainer container = new PointSetsContainer();
        public Layer(int layerId, double joinDist) { this.layerId = layerId; this.joinDist = joinDist; }     

        public void addPoint<PointType>(PointType point) where PointType : StaticPoint
        {
            List<PointSet<PointType>> sets = container.Get<PointType>();

            pointsCount++;
            double minDx, minDy, minDist;
            int minId = -1;
            minDist = minDx = minDy = joinDist;
            for (int i = 0; i < sets.Count; i++)
            {
                PointSet<PointType> set = sets[i];
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
                return;
            }
            PointSet<PointType> newSet = new PointSet<PointType>(point, joinDist);
            sets.Add(newSet);
            setsCount++;            
        }

        public void addSet<PointType>(PointSet<PointType> inSet) where PointType: StaticPoint
        {
            List<PointSet<PointType>> sets = container.Get<PointType>();

            pointsCount++;            
            foreach (PointSet<PointType> set in sets)
            {
                if (set.type != inSet.type)
                    continue;
                double dx = inSet.x - set.x;
                double dy = inSet.y - set.y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist <= joinDist)
                {
                    set.addSet(inSet, dx, dy);                    
                    return;
                }
            }
            sets.Add(inSet);
            setsCount++;
        }

        public PointSet<StaticPoint>[] getAllSets()
        {            
            int size = 0;
            foreach (IList list in container.Values)
                size += list.Count;
            PointSet<StaticPoint>[] sets = new PointSet<StaticPoint>[size];
            int index = 0;
            foreach (IList list in container.Values)
            {
                List<PointSet<StaticPoint>> listOfValues = list as List<PointSet<StaticPoint>>;
                if (listOfValues == null)
                    continue;
                listOfValues.CopyTo(sets, index);
                index += list.Count;
            }
            return sets;
        }
    }
}
