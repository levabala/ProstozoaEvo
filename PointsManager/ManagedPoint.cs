﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    abstract public class ManagedPoint
    {
        public double x, y;
        public double lpx, rpx, tpy, bpy;
        public long id;
        public int type;
        public double interactRadius;
        public Cluster[] clusters = new Cluster[0];

        public ManagedPoint(
            double x, double y, double interactRadius, long id, int type)
        {
            this.x = x;
            this.y = y;
            this.interactRadius = interactRadius;
            this.id = id;
            this.type = type;
            lpx = x - interactRadius;
            rpx = x + interactRadius;
            tpy = y - interactRadius;
            bpy = y + interactRadius;
        }

        public abstract void setClusters(Cluster[] clusters);        
    }
}