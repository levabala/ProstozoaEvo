using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class DynamicPointsManager
    {
        public readonly Pnt ZERO;
        public readonly double clusterSize;
        public Dictionary<long, DinamicPoint> points = new Dictionary<long, DinamicPoint>(); //int max is 2,147,483,647 so it's enough
        public Cluster[,] clusters;
        public Cluster zeroCluster;

        //debug
        public int li, ri, ti, bi, minLayerId;

        int clustersLeft, clustersRight, clustersTop, clustersBottom;
        public DynamicPointsManager(Pnt zeroPoint, double clusterSize)
        {
            clusters = new Cluster[1, 1];
            zeroCluster = clusters[0, 0] = new Cluster(zeroPoint.x, zeroPoint.y, clusterSize, 0, 0);
            ZERO = zeroPoint;
            this.clusterSize = clusterSize;
            clustersLeft = clustersRight = clustersTop = clustersBottom = 0;
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
            foreach (Cluster c in point.clusters)
                foreach (DinamicPoint p in c.points.Values)
                    nearPoints.Add(p);
            return nearPoints.ToArray();
            /*
            long[] ids = new long[count];
            int index = 0;
            foreach (Cluster c in point.clusters)
                foreach (DinamicPoint p in c.points.Values)
                    ids[index] = p.id;
            return ids;*/
        }

        public DinamicPoint[] getPoints(double lx, double rx, double ty, double by)
        {
            int[] ids = getClustersIdsByEdges(lx, rx, ty, by);
            return getPointsByIdBorders(ids[0], ids[1], ids[2], ids[3]);
        }

        public DinamicPointsSet[] getPointsSets(double lx, double rx, double ty, double by, int maxPointsCount)
        {
            int[] ids = getClustersIdsByEdges(lx, rx, ty, by);
            return getPointsSetsByIdBorders(ids[0], ids[1], ids[2], ids[3], maxPointsCount);
        }

        public DinamicPoint[] getPointsByIdBorders(int li, int ri, int ti, int bi)
        {            
            List<DinamicPoint> output = new List<DinamicPoint>();
            if (ri > clusters.GetLength(0) - 1)
                ri = clusters.GetLength(0) - 1;
            if (bi > clusters.GetLength(1) - 1)
                bi = clusters.GetLength(1) - 1;
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                    output.AddRange(clusters[idX, idY].points.Values);
            return output.ToArray(); ;
        }

        public DinamicPointsSet[] getPointsSetsByIdBorders(int li, int ri, int ti, int bi, int maxPointsCount)
        {            
            List<DinamicPointsSet> output = new List<DinamicPointsSet>();
            if (ri > clusters.GetLength(0) - 1)
                ri = clusters.GetLength(0) - 1;
            if (bi > clusters.GetLength(1) - 1)
                bi = clusters.GetLength(1) - 1;
            int clustersLeft = ri + bi - li - ti + 1;
            int minLayerId = 0;            
            //Cluster[,] clustersIn = new Cluster[ri - li + 1, bi - ti + 1];
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                {                    
                    int pointsPerCluster = (maxPointsCount - output.Count) / clustersLeft;
                    if (pointsPerCluster < 1)
                        pointsPerCluster = 1;
                    minLayerId = Math.Max(minLayerId, clusters[idX, idY].getLayerId(pointsPerCluster));
                    maxPointsCount -= clusters[idX, idY].layers[minLayerId].sets.Count;
                }
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)                                    
                    output.AddRange(clusters[idX, idY].layers[minLayerId].sets);
            this.minLayerId = minLayerId;
            return output.ToArray(); ;
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
                    newClusters[x, 0] = new Cluster(getClusterX(x), getClusterY(0), clusterSize, x, 0);
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
                    newClusters[x, maxY] = new Cluster(getClusterX(x), getClusterY(maxY), clusterSize, x, maxY);
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
                    newClusters[0, y] = new Cluster(getClusterX(0), getClusterY(y), clusterSize, 0, y);
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
                    newClusters[maxX, y] = new Cluster(getClusterX(maxX), getClusterY(y), clusterSize, maxX, y);
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
            DinamicPoint dpoint = points[id];
            updatePoint(dpoint, point.x - dpoint.x, point.y - dpoint.y, interactRadius);
        }

        private void updatePoint(DinamicPoint point)
        {
            point.setClusters(
                    getCluster(point.lpx, point.y),
                    getCluster(point.rpx, point.y),
                    getCluster(point.x, point.tpy),
                    getCluster(point.x, point.bpy)
                    );
        }
        private void updatePoint(DinamicPoint point, double dx, double dy, double interactRadius)
        {
            bool clusterCrossed = point.updateTriggers(dx, dy, interactRadius);
            if (clusterCrossed)
                updatePoint(point);
        }
    }

    public class Cluster
    {
        public Dictionary<long, DinamicPoint> points = new Dictionary<long, DinamicPoint>();
        public Layer[] layers;        
        public int idX, idY;
        public double x, y, size;
        public Cluster(double x, double y, double size, int idX, int idY, int layersCount = 100)
        {
            this.x = x;
            this.y = y;
            this.idX = idX;
            this.idY = idY;
            this.size = size;
            layers = new Layer[layersCount];
            double joinStep = size / layersCount;            
            //for (int i = layersCount - 1; i >= 0; i--)
            for (int i = 0; i < layers.Length; i++)                                          
                layers[i] = new Layer(i, joinStep * (i + 1));                            
        }

        private void addPointToLayer(DinamicPoint point, int layer)
        {
            Layer currLayer = layers[layer];
            bool nextAdd = currLayer.addPoint(point);            
        }

        public void addPoint(DinamicPoint point)
        {
            if (!points.ContainsKey(point.id))
                foreach (Layer layer in layers)
                    layer.addPoint(point);
            points[point.id] = point;            
        }

        public int getLayerId(int maxPointsCount)
        {
            for (int i = 0; i < layers.Length; i++)
                if (layers[i].sets.Count <= maxPointsCount)
                    return i;
            return layers.Length - 1;
        }

        public void removePoint(long id)
        {
            //here removing from layers, common dictionary..
        }

        public DinamicPointsSet[] getPointSets(int maxCount)
        {            
            for (int i = 0; i < layers.Length; i++)
                if (layers[i].sets.Count <= maxCount)
                    return layers[i].sets.ToArray();
            return layers.Last().sets.ToArray();
        }
    }

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
            foreach (DinamicPointsSet set in sets)
            {
                if (set.type != point.type)
                    continue;
                double dx = point.x - set.x;
                double dy = point.y - set.y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist <= joinDist)
                {
                    set.addPoint(point, dx, dy);                    
                    return pointsCount % (layerId + 2) == 0;
                }
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
    
    public class DinamicPoint
    {
        public double x, y, leftT, rightT, topT, bottomT;
        public double lpx, rpx, tpy, bpy;
        public long id;
        public int type;
        public double interactRadius;
        public bool isFreezed;
        public Cluster[] clusters;       
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
