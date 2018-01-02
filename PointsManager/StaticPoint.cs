using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class StaticPoint
    {
        public double x, y;        
        public long id;
        public int type;
        public double interactRadius;        
        public Cluster[] clusters;

        public StaticPoint(
            double x, double y, double interactRadius, long id, int type, Cluster[] clusters)
        {
            this.x = x;
            this.y = y;
            this.interactRadius = interactRadius;
            this.id = id;
            this.type = type;
            this.clusters = clusters;
            foreach (Cluster c in clusters)
                c.addStaticPoint(this);
        }
    }
}
