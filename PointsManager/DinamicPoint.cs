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
        public BaseCluster[] BaseClusters;
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

        BaseClusterEqualityComparer comparer = new BaseClusterEqualityComparer();
        public void setBaseClusters(BaseCluster lp, BaseCluster rp, BaseCluster tp, BaseCluster bp)
        {
            lpx = x - interactRadius;
            rpx = x + interactRadius;
            tpy = y - interactRadius;
            bpy = y + interactRadius;
            leftT = Math.Min(lpx - lp.x, lp.x + lp.size - lpx);
            rightT = Math.Min(rpx - rp.x, rp.x + rp.size - rpx);
            topT = Math.Min(tpy - tp.y, tp.y + tp.size - tpy);
            bottomT = Math.Min(bpy - bp.y, bp.y + bp.size - bpy);
            BaseCluster[] newBaseClusters = new BaseCluster[]
            {
                lp, rp, tp, bp
            };

            if (BaseClusters == null)
            {
                foreach (BaseCluster c in newBaseClusters)
                    c.addPoint(this);
                BaseClusters = newBaseClusters;
                return;
            }

            for (int i = 0; i < BaseClusters.Length; i++)
                if (BaseClusters[i].idX != newBaseClusters[i].idX || BaseClusters[i].idY != newBaseClusters[i].idY)
                {
                    BaseClusters[i].removePoint(id);
                    newBaseClusters[i].addPoint(this);
                }
            BaseClusters = newBaseClusters;
        }
    }
}
