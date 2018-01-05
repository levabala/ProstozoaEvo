using MathAssembly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class PointsManager
    {
        public readonly Pnt ZERO;
        public readonly double clusterSize;
        public int pointsCount = 0;        
        public DictionaryOfPointContainer pointsContainer = new DictionaryOfPointContainer();
        public Cluster[,] clusters;
        public Cluster zeroCluster;

        //debug
        public int li, ri, ti, bi, minLayerId;

        int clustersLeft, clustersRight, clustersTop, clustersBottom;
        int layersCount = 40;
        int clustersStep = 10;
        public PointsManager(Pnt zeroPoint, double clusterSize)
        {
            clusters = new Cluster[1, 1];
            zeroCluster = clusters[0, 0] = new Cluster(zeroPoint.x, zeroPoint.y, clusterSize, 0, 0, layersCount);
            ZERO = zeroPoint;
            this.clusterSize = clusterSize;
            clustersLeft = clustersRight = clustersTop = clustersBottom = 0;
        }

        public void addDinamicPoint(Pnt point, double interactRadius, long id, int type)
        {
            DinamicPoint p = new DinamicPoint(point.x, point.y, interactRadius, id, type);            
            updatePoint(p);
            pointsContainer.Get<DinamicPoint>()[id] = p;
            pointsCount++;
        }

        public void addStaticPoint(Pnt point, long id, int type)
        {
            StaticPoint p = new StaticPoint(point.x, point.y, 0, id, type);
            p.setClusters(getClusters(p));
            pointsContainer.Get<StaticPoint>()[id] = p;
            pointsCount++;
        }

        public long[] getNeighbors<PointType>(long id) where PointType : ManagedPoint
        {
            PointType point = null;
            if (!pointsContainer.Get<PointType>().TryGetValue(id, out point))
                return null;
            
            List<long> nearPoints = new List<long>();            
            foreach (Cluster c in point.clusters)
                foreach (PointType p in c.container.Get<PointType>().Values)
                    nearPoints.Add(p.id);

            return nearPoints.ToArray();            
        }

        public StaticPoint[] getPoints(double lx, double rx, double ty, double by)
        {
            int[] ids = getClustersIdsByEdges(lx, rx, ty, by);
            return getPointsByIdBorders(ids[0], ids[1], ids[2], ids[3]);
        }        

        public StaticPoint[] getPointsByIdBorders(int li, int ri, int ti, int bi)
        {            
            List<StaticPoint> output = new List<StaticPoint>();
            if (ri > clusters.GetLength(0) - 1)
                ri = clusters.GetLength(0) - 1;
            if (bi > clusters.GetLength(1) - 1)
                bi = clusters.GetLength(1) - 1;
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                    output.AddRange(clusters[idX, idY].getAllPoints());
            return output.ToArray(); ;
        }

        public PointSet<StaticPoint>[] getPointsSets(double lx, double rx, double ty, double by, int maxPointsCount)
        {
            int[] ids = getClustersIdsByEdges(lx, rx, ty, by);
            return getPointsSetsByIdBorders(ids[0], ids[1], ids[2], ids[3], maxPointsCount);
        }

        public PointSet<StaticPoint>[] getPointsSetsByIdBorders(int li, int ri, int ti, int bi, int maxPointsCount)
        {
            List<PointSet<StaticPoint>> output = new List<PointSet<StaticPoint>>();
            if (ri > clusters.GetLength(0) - 1)
                ri = clusters.GetLength(0) - 1;
            if (bi > clusters.GetLength(1) - 1)
                bi = clusters.GetLength(1) - 1;
            int totalClusters = (ri - li + 1) * (bi - ti + 1);
            int clustersLeft = totalClusters;
            int minLayerId = 0;
            int pointsCount = Int32.MaxValue;            

            while(pointsCount > maxPointsCount && minLayerId < layersCount)
            {
                pointsCount = 0;
                for (int idX = li; idX <= ri; idX++)
                    for (int idY = ti; idY <= bi; idY++)
                    {
                        pointsCount += clusters[idX, idY].layers[minLayerId].setsCount;
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

            //one-loop method
            /*for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                {
                    int pointsPerCluster = (maxPointsCount) / clustersLeft;
                    if (pointsPerCluster < 1)
                        pointsPerCluster = 1;
                    minLayerId = Math.Max(minLayerId, clusters[idX, idY].getLayerId(pointsPerCluster));
                    maxPointsCount -= clusters[idX, idY].layers[minLayerId].sets.Count;
                    clustersLeft--;
                }*/

            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)                                    
                    output.AddRange(clusters[idX, idY].layers[minLayerId].getAllSets());
            this.minLayerId = minLayerId;
            return output.ToArray();

            /*
             * int pointsPerCluster = maxPointsCount;
                    for (int idX2 = li; idX2 < idX; idX2++)
                        for (int idY2 = ti; idY2 <= bi; idY2++)
                            pointsPerCluster -= clusters[idX2, idY2].layers[minLayerId].sets.Count;                    
                    for (int idY2 = ti; idY2 <= idY; idY2++)
                        pointsPerCluster -= clusters[idX, idY2].layers[minLayerId].sets.Count;
                    pointsPerCluster /= clustersLeft;

                    if (pointsPerCluster < 1)
                        pointsPerCluster = 1;
                    minLayerId = Math.Max(minLayerId, clusters[idX, idY].getLayerId(pointsPerCluster));
                    clustersLeft--;
             */
        }

        private int[] getClustersIdsByEdges(double lx, double rx, double ty, double by)
        {
            int li = getClusterIdX(lx);
            int ri = getClusterIdX(rx);
            int ti = getClusterIdY(ty);
            int bi = getClusterIdY(by);
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

        private Cluster getCluster(double x, double y)
        {
            int idX = getClusterIdX(x);
            int idY = getClusterIdY(y);

            double clusterX = getClusterX(idX);
            double clusterY = getClusterY(idY);

            bool needToFill = idX < 0 || idY < 0 || idX >= clusters.GetLength(0) || idY >= clusters.GetLength(1) || clusters.Length == 0;
            if (needToFill)
            {
                fillClustersTo(idX, idY);
                return getCluster(x, y);
            }
            return clusters[idX, idY];
        }

        private int getClusterIdX(double x)
        {
            return (int)Math.Floor(x / clusterSize) + clustersLeft;
        }

        private int getClusterIdY(double y)
        {
            return (int)Math.Floor(y / clusterSize) + clustersTop;
        }

        private void fillClustersTo(int idx, int idy)
        {
            int addLeft = -idx;
            int addRight = idx - clusters.GetLength(0) + 1;
            int addTop = -idy;
            int addBottom = idy - clusters.GetLength(1) + 1;
            while (addLeft > 0)
            {
                addClustersColumn(true);
                addLeft--;
            }
            while (addRight > 0)
            {
                addClustersColumn(false);
                addRight--;
            }
            while (addTop > 0)
            {
                addClustersRow(true);
                addTop--;
            }
            while (addBottom > 0)
            {
                addClustersRow(false);
                addBottom--;
            }
        }

        private void addClustersRow(bool before)
        {
            Cluster[,] newClusters = new Cluster[clusters.GetLength(0), clusters.GetLength(1) + 1];
            if (before)
            {
                clustersTop++;
                for (int x = 0; x < newClusters.GetLength(0); x++)
                {
                    newClusters[x, 0] = new Cluster(getClusterX(x), getClusterY(0), clusterSize, x, 0, layersCount);
                    for (int y = 1; y < clusters.GetLength(1) + 1; y++)
                    {
                        newClusters[x, y] = clusters[x, y - 1];
                        newClusters[x, y].idY = y;
                        newClusters[x, y].idX = x;
                    }
                }
            }
            else
            {
                clustersBottom++;
                for (int x = 0; x < newClusters.GetLength(0); x++)
                {
                    int maxY = newClusters.GetLength(1) - 1;
                    newClusters[x, maxY] = new Cluster(getClusterX(x), getClusterY(maxY), clusterSize, x, maxY, layersCount);
                    for (int y = 0; y < clusters.GetLength(1); y++)
                    {
                        newClusters[x, y] = clusters[x, y];
                        newClusters[x, y].idY = y;
                        newClusters[x, y].idX = x;
                    }
                }
            }
            clusters = newClusters;
        }

        private void addClustersColumn(bool before)
        {
            Cluster[,] newClusters = new Cluster[clusters.GetLength(0) + 1, clusters.GetLength(1)];
            if (before)
            {
                clustersLeft++;
                for (int y = 0; y < newClusters.GetLength(1); y++)
                {
                    newClusters[0, y] = new Cluster(getClusterX(0), getClusterY(y), clusterSize, 0, y, layersCount);
                    for (int x = 1; x < clusters.GetLength(0) + 1; x++)
                    {
                        newClusters[x, y] = clusters[x - 1, y];
                        newClusters[x, y].idY = y;
                        newClusters[x, y].idX = x;
                    }
                }
            }
            else
            {
                clustersRight++;
                for (int y = 0; y < newClusters.GetLength(1); y++)
                {
                    int maxX = newClusters.GetLength(0) - 1;
                    newClusters[maxX, y] = new Cluster(getClusterX(maxX), getClusterY(y), clusterSize, maxX, y, layersCount);
                    for (int x = 0; x < clusters.GetLength(0); x++)
                    {
                        newClusters[x, y] = clusters[x, y];
                        newClusters[x, y].idY = y;
                        newClusters[x, y].idX = x;
                    }
                }
            }
            clusters = newClusters;
        }        
        
        private double getClusterX(int id)
        {
            return clusterSize * (id - clustersLeft);
        }

        private double getClusterY(int id)
        {
            return clusterSize * (id - clustersTop);
        }

        public void updatePoint(Pnt point, double interactRadius, int id)
        {
            DinamicPoint dpoint = pointsContainer.Get<DinamicPoint>()[id];
            updatePoint(dpoint, point.x - dpoint.x, point.y - dpoint.y, interactRadius);
        }

        private Cluster[] getClusters(double lpx, double x, double rpx, double tpy, double y, double bpy)
        {
            return new Cluster[]
            {
                getCluster(lpx, y),
                getCluster(rpx, y),
                getCluster(x, tpy),
                getCluster(x, bpy)
            };
        }

        private Cluster[] getClusters(ManagedPoint point)
        {
            return new Cluster[]
            {
                getCluster(point.lpx, point.y),
                getCluster(point.rpx, point.y),
                getCluster(point.x, point.tpy),
                getCluster(point.x, point.bpy)
            };
        }

        private void updatePoint(DinamicPoint point)
        {
            Cluster[] clusters = getClusters(point);
            point.setClusters(clusters[0], clusters[1], clusters[2], clusters[3]);
        }
        private void updatePoint(DinamicPoint point, double dx, double dy, double interactRadius)
        {
            bool clusterCrossed = point.updateTriggers(dx, dy, interactRadius);
            if (clusterCrossed)
                updatePoint(point);
        }
    }                  

    class ClusterEqualityComparer : IEqualityComparer<Cluster>
    {
        public bool Equals(Cluster c1, Cluster c2)
        {
            return c1.idX == c2.idX && c1.idY == c2.idY;
        }

        public int GetHashCode(Cluster cluster)
        {
            int hCode = cluster.idX ^ cluster.idY;
            return hCode.GetHashCode();
        }
    }
}
