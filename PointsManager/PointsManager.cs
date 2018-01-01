using MathAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class PointsManager
    {
        public readonly Pnt ZERO;
        public readonly double BaseClusterSize;
        public Dictionary<long, DinamicPoint> points = new Dictionary<long, DinamicPoint>(); //int max is 2,147,483,647 so it's enough
        public BaseCluster[,] BaseClusters;
        public BaseCluster zeroBaseCluster;

        //debug
        public int li, ri, ti, bi, minLayerId;

        int BaseClustersLeft, BaseClustersRight, BaseClustersTop, BaseClustersBottom;
        int layersCount = 40;
        int BaseClustersStep = 10;
        public PointsManager(Pnt zeroPoint, double BaseClusterSize)
        {
            BaseClusters = new BaseCluster[1, 1];
            zeroBaseCluster = BaseClusters[0, 0] = new BaseCluster(zeroPoint.x, zeroPoint.y, BaseClusterSize, 0, 0, layersCount);
            ZERO = zeroPoint;
            this.BaseClusterSize = BaseClusterSize;
            BaseClustersLeft = BaseClustersRight = BaseClustersTop = BaseClustersBottom = 0;
        }

        public void addPoint(Pnt point, double interactRadius, long id, int type)
        {
            DinamicPoint p = new DinamicPoint(point.x, point.y, interactRadius, id, type);            
            updatePoint(p);
            points[id] = p;
        }

        public void addStaticPoint(Pnt point, long id, int type)
        {
            DinamicPoint p = new DinamicPoint(point.x, point.y, 0, id, type, true);            
            updatePoint(p);
            points[id] = p;
        }
        
        public DinamicPoint[] getNeighbors(long id)
        {
            DinamicPoint point = points[id];
            //int count = 0;
            List<DinamicPoint> nearPoints = new List<DinamicPoint>();
            foreach (BaseCluster c in point.BaseClusters)
            {                                
                foreach (long pointId in c.storedPoints)
                    nearPoints.Add(points[pointId]);
            }
            return nearPoints.ToArray();
        }

        /*
        public DinamicPoint[] getPoints(double lx, double rx, double ty, double by)
        {
            int[] ids = getBaseClustersIdsByEdges(lx, rx, ty, by);
            return getPointsByIdBorders(ids[0], ids[1], ids[2], ids[3]);
        }


        public DinamicPoint[] getPointsByIdBorders(int li, int ri, int ti, int bi)
        {            
            List<DinamicPoint> output = new List<DinamicPoint>();
            if (ri > BaseClusters.GetLength(0) - 1)
                ri = BaseClusters.GetLength(0) - 1;
            if (bi > BaseClusters.GetLength(1) - 1)
                bi = BaseClusters.GetLength(1) - 1;
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                    output.AddRange(BaseClusters[idX, idY].points.Values);
            return output.ToArray(); ;
        }*/

        public DinamicPointsSet[] getPointsSets(double lx, double rx, double ty, double by, int maxPointsCount)
        {
            int[] ids = getBaseClustersIdsByEdges(lx, rx, ty, by);
            return getPointsSetsByIdBorders(ids[0], ids[1], ids[2], ids[3], maxPointsCount, new Pnt(lx, ty), new Pnt(rx, by));
        }

        public DinamicPointsSet[] getPointsSetsByIdBorders(int li, int ri, int ti, int bi, int maxPointsCount, Pnt leftTop, Pnt rightBottom)
        {                     
            List<DinamicPointsSet> output = new List<DinamicPointsSet>();
            if (ri > BaseClusters.GetLength(0) - 1)
                ri = BaseClusters.GetLength(0) - 1;
            if (bi > BaseClusters.GetLength(1) - 1)
                bi = BaseClusters.GetLength(1) - 1;
            int totalBaseClusters = (ri - li + 1) * (bi - ti + 1);

            //method with sub-clusters
            int pointsPerCluster = maxPointsCount / totalBaseClusters;
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                    output.AddRange(BaseClusters[idX, idY].getPointSets(leftTop, rightBottom, pointsPerCluster));
            return output.ToArray();

            //method without sub-clusters
            /*
            int minLayerId = 0;
            int pointsCount = Int32.MaxValue;
            while(pointsCount > maxPointsCount && minLayerId < layersCount)
            {
                pointsCount = 0;
                for (int idX = li; idX <= ri; idX++)
                    for (int idY = ti; idY <= bi; idY++)
                    {
                        pointsCount += BaseClusters[idX, idY].layers[minLayerId].sets.Count;
                        if (pointsCount > maxPointsCount)
                        {                            
                            //init exit
                            idX = ri + 1;                            
                            minLayerId++;
                            break;
                        }                            
                    }
            }
            if (minLayerId >= layersCount)
                minLayerId = layersCount - 1;

            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)                                    
                    output.AddRange(BaseClusters[idX, idY].layers[minLayerId].sets);
            this.minLayerId = minLayerId;
            return output.ToArray(); ;                
             */

                    //one-loop method
                    /*for (int idX = li; idX <= ri; idX++)
                        for (int idY = ti; idY <= bi; idY++)
                        {
                            int pointsPerBaseCluster = (maxPointsCount) / BaseClustersLeft;
                            if (pointsPerBaseCluster < 1)
                                pointsPerBaseCluster = 1;
                            minLayerId = Math.Max(minLayerId, BaseClusters[idX, idY].getLayerId(pointsPerBaseCluster));
                            maxPointsCount -= BaseClusters[idX, idY].layers[minLayerId].sets.Count;
                            BaseClustersLeft--;
                        }*/

                    /*
                     * int pointsPerBaseCluster = maxPointsCount;
                            for (int idX2 = li; idX2 < idX; idX2++)
                                for (int idY2 = ti; idY2 <= bi; idY2++)
                                    pointsPerBaseCluster -= BaseClusters[idX2, idY2].layers[minLayerId].sets.Count;                    
                            for (int idY2 = ti; idY2 <= idY; idY2++)
                                pointsPerBaseCluster -= BaseClusters[idX, idY2].layers[minLayerId].sets.Count;
                            pointsPerBaseCluster /= BaseClustersLeft;

                            if (pointsPerBaseCluster < 1)
                                pointsPerBaseCluster = 1;
                            minLayerId = Math.Max(minLayerId, BaseClusters[idX, idY].getLayerId(pointsPerBaseCluster));
                            BaseClustersLeft--;
                     */
        }

        private int[] getBaseClustersIdsByEdges(double lx, double rx, double ty, double by)
        {
            int li = getBaseClusterIdX(lx);
            int ri = getBaseClusterIdX(rx);
            int ti = getBaseClusterIdY(ty);
            int bi = getBaseClusterIdY(by);
            if (li < 0)
                li = 0;
            if (ri < 0)
                ri = 0;
            if (ti < 0)
                ti = 0;
            if (bi < 0)
                bi = 0;
            this.li = li;
            this.ri = ri;
            this.ti = ti;
            this.bi = bi;
            return new int[] { li, ri, ti, bi };
        }

        private BaseCluster getBaseCluster(double x, double y)
        {
            int idX = getBaseClusterIdX(x);
            int idY = getBaseClusterIdY(y);

            double BaseClusterX = getBaseClusterX(idX);
            double BaseClusterY = getBaseClusterY(idY);

            bool needToFill = idX < 0 || idY < 0 || idX >= BaseClusters.GetLength(0) || idY >= BaseClusters.GetLength(1) || BaseClusters.Length == 0;
            if (needToFill)
            {
                fillBaseClustersTo(idX, idY);
                return getBaseCluster(x, y);
            }
            return BaseClusters[idX, idY];
        }

        private int getBaseClusterIdX(double x)
        {
            return (int)Math.Floor(x / BaseClusterSize) + BaseClustersLeft;
        }

        private int getBaseClusterIdY(double y)
        {
            return (int)Math.Floor(y / BaseClusterSize) + BaseClustersTop;
        }

        private void fillBaseClustersTo(int idx, int idy)
        {
            int addLeft = -idx;
            int addRight = idx - BaseClusters.GetLength(0) + 1;
            int addTop = -idy;
            int addBottom = idy - BaseClusters.GetLength(1) + 1;
            while (addLeft > 0)
            {
                addBaseClustersColumn(true);
                addLeft--;
            }
            while (addRight > 0)
            {
                addBaseClustersColumn(false);
                addRight--;
            }
            while (addTop > 0)
            {
                addBaseClustersRow(true);
                addTop--;
            }
            while (addBottom > 0)
            {
                addBaseClustersRow(false);
                addBottom--;
            }
        }

        private void addBaseClustersRow(bool before)
        {
            BaseCluster[,] newBaseClusters = new BaseCluster[BaseClusters.GetLength(0), BaseClusters.GetLength(1) + 1];
            if (before)
            {
                BaseClustersTop++;
                for (int x = 0; x < newBaseClusters.GetLength(0); x++)
                {
                    newBaseClusters[x, 0] = new BaseCluster(getBaseClusterX(x), getBaseClusterY(0), BaseClusterSize, x, 0, layersCount);
                    for (int y = 1; y < BaseClusters.GetLength(1) + 1; y++)
                    {
                        newBaseClusters[x, y] = BaseClusters[x, y - 1];
                        newBaseClusters[x, y].idY = y;
                        newBaseClusters[x, y].idX = x;
                    }
                }
            }
            else
            {
                BaseClustersBottom++;
                for (int x = 0; x < newBaseClusters.GetLength(0); x++)
                {
                    int maxY = newBaseClusters.GetLength(1) - 1;
                    newBaseClusters[x, maxY] = new BaseCluster(getBaseClusterX(x), getBaseClusterY(maxY), BaseClusterSize, x, maxY, layersCount);
                    for (int y = 0; y < BaseClusters.GetLength(1); y++)
                    {
                        newBaseClusters[x, y] = BaseClusters[x, y];
                        newBaseClusters[x, y].idY = y;
                        newBaseClusters[x, y].idX = x;
                    }
                }
            }
            BaseClusters = newBaseClusters;
        }

        private void addBaseClustersColumn(bool before)
        {
            BaseCluster[,] newBaseClusters = new BaseCluster[BaseClusters.GetLength(0) + 1, BaseClusters.GetLength(1)];
            if (before)
            {
                BaseClustersLeft++;
                for (int y = 0; y < newBaseClusters.GetLength(1); y++)
                {
                    newBaseClusters[0, y] = new BaseCluster(getBaseClusterX(0), getBaseClusterY(y), BaseClusterSize, 0, y, layersCount);
                    for (int x = 1; x < BaseClusters.GetLength(0) + 1; x++)
                    {
                        newBaseClusters[x, y] = BaseClusters[x - 1, y];
                        newBaseClusters[x, y].idY = y;
                        newBaseClusters[x, y].idX = x;
                    }
                }
            }
            else
            {
                BaseClustersRight++;
                for (int y = 0; y < newBaseClusters.GetLength(1); y++)
                {
                    int maxX = newBaseClusters.GetLength(0) - 1;
                    newBaseClusters[maxX, y] = new BaseCluster(getBaseClusterX(maxX), getBaseClusterY(y), BaseClusterSize, maxX, y, layersCount);
                    for (int x = 0; x < BaseClusters.GetLength(0); x++)
                    {
                        newBaseClusters[x, y] = BaseClusters[x, y];
                        newBaseClusters[x, y].idY = y;
                        newBaseClusters[x, y].idX = x;
                    }
                }
            }
            BaseClusters = newBaseClusters;
        }        
        
        private double getBaseClusterX(int id)
        {
            return BaseClusterSize * (id - BaseClustersLeft);
        }

        private double getBaseClusterY(int id)
        {
            return BaseClusterSize * (id - BaseClustersTop);
        }

        public void updatePoint(Pnt point, double interactRadius, int id)
        {
            DinamicPoint dpoint = points[id];
            updatePoint(dpoint, point.x - dpoint.x, point.y - dpoint.y, interactRadius);
        }

        private void updatePoint(DinamicPoint point)
        {
            point.setBaseClusters(
                    getBaseCluster(point.lpx, point.y),
                    getBaseCluster(point.rpx, point.y),
                    getBaseCluster(point.x, point.tpy),
                    getBaseCluster(point.x, point.bpy)
                    );
        }
        private void updatePoint(DinamicPoint point, double dx, double dy, double interactRadius)
        {
            bool BaseClusterCrossed = point.updateTriggers(dx, dy, interactRadius);
            if (BaseClusterCrossed)
                updatePoint(point);
        }
    }                  

    class BaseClusterEqualityComparer : IEqualityComparer<BaseCluster>
    {
        public bool Equals(BaseCluster c1, BaseCluster c2)
        {
            return c1.idX == c2.idX && c1.idY == c2.idY;
        }

        public int GetHashCode(BaseCluster BaseCluster)
        {
            int hCode = BaseCluster.idX ^ BaseCluster.idY;
            return hCode.GetHashCode();
        }
    }
}
