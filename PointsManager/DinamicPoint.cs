using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class DinamicPoint
    {
        public double x, y, leftT, rightT, topT, bottomT;
        public double lpx, rpx, tpy, bpy;
        public long id;
        public int type;
        public double interactRadius;
        public bool isFreezed;
        public Cluster[] originClusters;
        public List<Cluster> upscaleClusters = new List<Cluster>();
        public DinamicPoint(double x, double y, double interactRadius, long id, int type, bool isFreezed = false)
        {
            this.x = x;
            this.y = y;
            this.interactRadius = interactRadius;
            this.id = id;
            this.type = type;
            this.isFreezed = isFreezed;
            leftT = rightT = topT = bottomT = 0;
            lpx = x - interactRadius;
            rpx = x + interactRadius;
            tpy = y - interactRadius;
            bpy = y + interactRadius;
        }

        public bool updateTriggers(double dx, double dy, double interactRadius)
        {
            double radiusDelta = interactRadius - this.interactRadius;
            this.interactRadius = interactRadius;
            leftT += dx;
            rightT -= dx;
            topT += dy;
            bottomT -= dy;
            leftT -= radiusDelta;
            rightT -= radiusDelta;
            topT -= radiusDelta;
            bottomT -= radiusDelta;
            return leftT < 0 || rightT < 0 || topT < 0 || bottomT < 0;
        }

        ClusterEqualityComparer comparer = new ClusterEqualityComparer();
        public void setClusters(Cluster lp, Cluster rp, Cluster tp, Cluster bp)
        {
            lpx = x - interactRadius;
            rpx = x + interactRadius;
            tpy = y - interactRadius;
            bpy = y + interactRadius;
            leftT = Math.Min(lpx - lp.x, lp.x + lp.size - lpx);
            rightT = Math.Min(rpx - rp.x, rp.x + rp.size - rpx);
            topT = Math.Min(tpy - tp.y, tp.y + tp.size - tpy);
            bottomT = Math.Min(bpy - bp.y, bp.y + bp.size - bpy);
            Cluster[] newClusters = new Cluster[]
            {
                lp, rp, tp, bp
            };

            if (clusters == null)
            {
                foreach (Cluster c in newClusters)
                    c.addPoint(this);
                clusters = newClusters;
                return;
            }

            for (int i = 0; i < clusters.Length; i++)
                if (clusters[i].idX != newClusters[i].idX || clusters[i].idY != newClusters[i].idY)
                {
                    clusters[i].removePoint(id);
                    newClusters[i].addPoint(this);
                }
            clusters = newClusters;
        }
    }
}
