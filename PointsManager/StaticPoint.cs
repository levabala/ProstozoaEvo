using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillionPointsManager
{
    public class StaticPoint : ManagedPoint
    {        
        public StaticPoint(
            double x, double y, double interactRadius, long id, int type)
            : base(x,y,interactRadius,id,type)
        {
            
        }

        public override void setClusters(Cluster[] clusters)
        {
            this.clusters = clusters;
            foreach (Cluster c in clusters)
                c.addPoint(this);
        }

        public override void addCluster(Cluster c) {
            Cluster[] newClusters = new Cluster[clusters.Length + 1];
            clusters.CopyTo(newClusters, 0);
            newClusters[newClusters.Length - 1] = c;
            clusters = newClusters;
            c.addPoint(this);
        }
    }
}
