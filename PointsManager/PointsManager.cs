using MathAssembly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillionPointsManager
{
    public class PointsManager
    {
        public readonly Pnt ZERO;
        public readonly double clusterSize;
        public readonly double lowestPointSize = 0;
        public bool isGeneratingLayer = false;
        public double layerGeneratingProgress = 0;
        public int pointsCount = 0;
        public int fixedLayerId = -1;
        public DictionaryOfPointContainer pointsContainer = new DictionaryOfPointContainer();
        public Cluster[,,] clusters;
        public Cluster zeroCluster;

        //debug
        public int li, ri, ti, bi, minLayerId;

        int clustersLeft, clustersRight, clustersTop, clustersBottom;
        int layersCount = 60;
        int deepScale = 2;
        public PointsManager(Pnt zeroPoint, double clusterSize)
        {            
            lowestPointSize = clusterSize / layersCount;
            clusters = new Cluster[1, 1, 1];
            zeroCluster = clusters[0, 0, 0] = new Cluster(zeroPoint.x, zeroPoint.y, clusterSize, 0, 0, 0, layersCount);
            ZERO = zeroPoint;
            this.clusterSize = clusterSize;
            clustersLeft = clustersRight = clustersTop = clustersBottom = 0;

            /*lock (fillLocker)
            {
                addClustersLayer(ref clusters, ref layerGeneratingProgress);
                addClustersLayer(ref clusters, ref layerGeneratingProgress);
            }*/
        }

        public void addDinamicPoint(Pnt point, double interactRadius, long id, int type, params KeyValuePair<Object, Object>[] linkedObjects)
        {
            DinamicPoint p = new DinamicPoint(point.x, point.y, interactRadius, id, type);
            foreach (KeyValuePair<Object, Object> pair in linkedObjects)
                p.linkObject(pair.Key, pair.Value);
            updatePoint(p);
            pointsContainer.Get<DinamicPoint>()[id] = p;
            pointsCount++;
        }
        
        public void addStaticPoint(Pnt point, long id, int type, params KeyValuePair<Object, Object>[] linkedObjects)
        {
            StaticPoint p = new StaticPoint(point.x, point.y, 0, id, type);
            foreach (KeyValuePair<Object, Object> pair in linkedObjects)
                p.linkObject(pair.Key, pair.Value);
            p.setClusters(getClusters(p));
            pointsContainer.Get<StaticPoint>()[id] = p;
            pointsCount++;
        }

        public long[] getNeighborsIds<PointType>(long id) where PointType : ManagedPoint
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
        /*
        public ManagedPoint[] getPoints(double lx, double rx, double ty, double by)
        {
            int[] ids = getClustersIdsByEdges(lx, rx, ty, by);
            return getPointsByIdBorders(ids[0], ids[1], ids[2], ids[3]);
        }        

        public ManagedPoint[] getPointsByIdBorders(int li, int ri, int ti, int bi)
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
        }*/

        public PointSet[] getPointsSets(double lx, double rx, double ty, double by, int maxPointsCount)
        {
            int[] ids = getClustersIdsByEdges(lx, rx, ty, by, maxPointsCount);
            return getPointsSetsByIdBorders(ids[0], ids[1], ids[2], ids[3], ids[4], maxPointsCount);
        }

        public PointSet[] getPointsSetsByIdBorders(int li, int ri, int ti, int bi, int deepnees, int maxPointsCount)
        {
            //return OneByOneGetterNotSync(li, ri, ti, bi, maxPointsCount);
            //lock (fillLocker)            
                return MinLayerOneLoopGetter(li, ri, ti, bi, deepnees, maxPointsCount);
        }        

        private PointSet[] OneByOneGetterNotSync(int li, int ri, int ti, int bi, int deepnees, int maxPointsCount)
        {
            int clustersLeft = (ri - li + 1) * (bi - ti + 1);
            List<PointSet> output = new List<PointSet>();
            int pointsLeft = maxPointsCount;
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                {
                    output.AddRange(clusters[idX, idY, deepnees].getPointSets<StaticPoint>(pointsLeft / clustersLeft));
                    clustersLeft--;
                    pointsLeft = maxPointsCount - output.Count;
                }
            return output.ToArray();
        }

        private PointSet[] MinLayerOneLoopGetter(int li, int ri, int ti, int bi, int deepnees, int maxPointsCount)
        {
            int clustersLeft = (ri - li + 1) * (bi - ti + 1);
            int minLayerId = 0;
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                {
                    int pointsPerCluster = (maxPointsCount) / clustersLeft;
                    if (pointsPerCluster < 1)
                        pointsPerCluster = 1;
                    int got = clusters[idX, idY, deepnees].getLayerId<StaticPoint>(pointsPerCluster);
                    int last = minLayerId;
                    minLayerId = Math.Max(minLayerId, got);
                    maxPointsCount -= clusters[idX, idY, deepnees].layers[minLayerId].setsCount;
                    clustersLeft--;
                }
            return getAllAtLayer(li, ri, ti, bi, deepnees, minLayerId);
        }

        private PointSet[] MinLayerGetter(int li, int ri, int ti, int bi, int deepnees, int maxPointsCount)
        {            
            int minLayerId = 0;
            int pointsCount = Int32.MaxValue;
            while (pointsCount > maxPointsCount && minLayerId < layersCount)
            {
                pointsCount = 0;
                for (int idX = li; idX <= ri; idX++)
                    for (int idY = ti; idY <= bi; idY++)
                    {
                        pointsCount += clusters[idX, idY, deepnees].layers[minLayerId].setsCount;
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
            this.minLayerId = minLayerId;

            return getAllAtLayer(li, ri, ti, bi, deepnees, minLayerId);
        }

        private PointSet[] getAllAtLayer(int li, int ri, int ti, int bi, int deepnees, int layerId)
        {
            List<PointSet> output = new List<PointSet>();
            for (int idX = li; idX <= ri; idX++)
                for (int idY = ti; idY <= bi; idY++)
                    output.AddRange(clusters[idX, idY, deepnees].layers[layerId].getAllSets());            
            return output.ToArray();
        }

        private int[] getClustersIdsByEdges(double lx, double rx, double ty, double by, int maxPointsCount)
        {
            int li = getClusterIdX(lx, 0);
            int ri = getClusterIdX(rx, 0);
            int ti = getClusterIdY(ty, 0);
            int bi = getClusterIdY(by, 0);
            if (li < 0)
                li = 0;
            if (ri < 0)
                ri = 0;
            if (ti < 0)
                ti = 0;
            if (bi < 0)
                bi = 0;
            if (ri > clusters.GetLength(0) - 1)
                ri = clusters.GetLength(0) - 1;
            if (bi > clusters.GetLength(1) - 1)
                bi = clusters.GetLength(1) - 1;

            int deepnees = 0;
            if (fixedLayerId >= 0)
            {
                deepnees = fixedLayerId;
                int step = (int)Math.Pow(deepScale, deepnees);
                li = (int)Math.Ceiling((double)li / step);
                ri = (int)Math.Ceiling((double)ri / step);
                ti = (int)Math.Ceiling((double)ti / step);
                bi = (int)Math.Ceiling((double)bi / step);                
            }
            else
                while ((ri - li + 1) * (bi - ti + 1) > maxPointsCount)
                {
                    li /= deepScale;
                    ri /= deepScale;
                    ti /= deepScale;
                    bi /= deepScale;
                    deepnees++;
                }

            if (deepnees + 1 > clusters.GetLength(2))
            {
                deepnees = clusters.GetLength(2) - 1;
                if (!isGeneratingLayer)
                {
                    new Task(() =>
                    {
                        Cluster[,,] newClusters;
                        lock (fillLocker)
                        {
                            newClusters = clusters.Clone() as Cluster[,,];
                            addClustersLayer(ref newClusters, ref layerGeneratingProgress);
                            layerGeneratingProgress = 1;
                            clusters = newClusters;
                        }
                    }).Start();
                }
            }

            li = getClusterIdX(lx, deepnees);
            ri = getClusterIdX(rx, deepnees);
            ti = getClusterIdY(ty, deepnees);
            bi = getClusterIdY(by, deepnees);
            if (li < 0)
                li = 0;
            if (ri < 0)
                ri = 0;
            if (ti < 0)
                ti = 0;
            if (bi < 0)
                bi = 0;
            int xMax = (int)Math.Ceiling((clusters.GetLength(0) - 1) / Math.Pow(deepScale, deepnees));
            int yMax = (int)Math.Ceiling((clusters.GetLength(1) - 1) / Math.Pow(deepScale, deepnees));
            if (ri > xMax)
                ri = xMax;
            if (bi > yMax)
                bi = yMax;

            //<for debug>
            this.li = li;
            this.ri = ri;
            this.ti = ti;
            this.bi = bi;
            //<for debug/>

            return new int[] { li, ri, ti, bi, deepnees };
        }

        object fillLocker = new object();
        private Cluster getCluster(double x, double y, int deepnees)
        {            
            int idX = getClusterIdX(x, deepnees);
            int idY = getClusterIdY(y, deepnees);

            double clusterX = getClusterX(idX, deepnees);
            double clusterY = getClusterY(idY, deepnees);

            bool needToFill = idX < 0 || idY < 0 || idX >= clusters.GetLength(0) || idY >= clusters.GetLength(1) || clusters.Length == 0;
            if (needToFill)
            {
                //lock (fillLocker)
                //{
                    Cluster[,,] newClusters;
                    newClusters = clusters.Clone() as Cluster[,,];

                    fillClustersTo(ref newClusters, idX, idY);
                    clusters = newClusters;
                    return getCluster(x, y, deepnees);
                //}
            }
            return clusters[idX, idY, deepnees];
        }

        private int getClusterIdX(double x, int deepnees)
        {            
            return (int)(x / (clusterSize * (int)Math.Pow(deepScale, deepnees))) + clustersLeft / (int)Math.Pow(deepScale, deepnees);
        }

        private int getClusterIdY(double y, int deepnees)
        {
            return (int)(y / (clusterSize * (int)Math.Pow(deepScale, deepnees))) + clustersTop / (int)Math.Pow(deepScale, deepnees);
        }

        private void fillClustersTo(ref Cluster[,,] clusters, int idx, int idy)
        {
            int addLeft = -idx;
            int addRight = idx - clusters.GetLength(0) + 1;
            int addTop = -idy;
            int addBottom = idy - clusters.GetLength(1) + 1;
            while (addLeft > 0)
            {
                addClustersColumn(ref clusters, true);
                addLeft--;
            }
            while (addRight > 0)
            {
                addClustersColumn(ref clusters, false);
                addRight--;
            }
            while (addTop > 0)
            {
                addClustersRow(ref clusters, true);
                addTop--;
            }
            while (addBottom > 0)
            {
                addClustersRow(ref clusters, false);
                addBottom--;
            }
        }

        private void addClustersLayer(ref Cluster[,,] clusters, ref double progress)
        {
            isGeneratingLayer = true;
            int newDeepnees = clusters.GetLength(2);            
            Cluster[,,] newClusters = new Cluster[clusters.GetLength(0), clusters.GetLength(1), newDeepnees + 1];            

            //clone
            for (int idZ = 0; idZ < clusters.GetLength(2); idZ++)
            {
                int xMax = (int)Math.Ceiling((clusters.GetLength(0) - 1) / Math.Pow(deepScale, idZ));
                for (int idX = 0; idX <= xMax; idX++)
                {
                    int yMax = (int)Math.Ceiling((clusters.GetLength(1) - 1) / Math.Pow(deepScale, idZ));
                    for (int idY = 0; idY <= yMax; idY++)
                        newClusters[idX, idY, idZ] = clusters[idX, idY, idZ];
                }
            }

            //add layer (fuuuck.. -_-)        
            int previousLayerId = clusters.GetLength(2) - 1;
            int step = (int)Math.Pow(deepScale, newDeepnees);

            int maxX = clusters.GetLength(0) - 1;
            int maxY = clusters.GetLength(1) - 1;

            int idXlowMax = clusters.GetLength(0) - 1;// / (int)Math.Pow(deepScale, newDeepnees);
            int idYlowMax = clusters.GetLength(1) - 1;// / (int)Math.Pow(deepScale, newDeepnees);
            idXlowMax = (int)Math.Ceiling((double)idXlowMax / step) * step;
            idYlowMax = (int)Math.Ceiling((double)idYlowMax / step) * step;

            int allWork = idYlowMax * idXlowMax / step / step;
            double workDone = 0;

            for (int idXlow = 0; idXlow <= idXlowMax; idXlow += step)
                for (int idYlow = 0; idYlow <= idYlowMax; idYlow += step)
                {
                    int idX = idXlow / step;
                    int idY = idYlow / step;                                                               

                    Cluster newCluster =
                        new Cluster(
                            getClusterX(idX, newDeepnees),
                            getClusterY(idY, newDeepnees),
                            clusterSize * Math.Pow(deepScale, newDeepnees),
                            idX, idY, newDeepnees, layersCount
                        );
                    
                    int toX = Math.Min(idXlow + step, maxX);
                    int toY = Math.Min(idYlow + step, maxY);
                    int lowStep = (int)Math.Pow(deepScale, previousLayerId);
                    for (int addX = idXlow; addX <= toX; addX += 1)
                        for (int addY = idYlow; addY <= toY; addY += 1)
                        {
                            Cluster c = clusters[addX, addY, previousLayerId];
                            if (c == null)
                                continue;
                            foreach (ManagedPoint p in c.getAllPointsAsArray())
                                p.addCluster(newCluster);
                        }

                    newClusters[idX, idY, newDeepnees] = newCluster;

                    workDone++;
                    progress = workDone / allWork;
                }
            clusters = newClusters;
            isGeneratingLayer = false;
        }

        private void addClustersRow(ref Cluster[,,] clusters, bool before)
        {
            Cluster[,,] newClusters = new Cluster[clusters.GetLength(0), clusters.GetLength(1) + 1, clusters.GetLength(2)];            
            if (before)
            {
                clustersTop++;
                for (int z = 0; z < newClusters.GetLength(2); z++)
                {
                    int step = (int)Math.Pow(deepScale, z);
                    int xLimit = (int)Math.Ceiling((double)newClusters.GetLength(0) / step);
                    for (int x = 0; x < xLimit; x++)
                    {
                        int yLimit = (int)Math.Ceiling((double)clusters.GetLength(1) / step) + 1;
                        newClusters[x, 0, z] = new Cluster(getClusterX(x, z), getClusterY(0, z), clusterSize * Math.Pow(deepScale, z), x, 0, z, layersCount);
                        for (int y = 1; y < yLimit; y++)
                        {
                            Cluster c = clusters[x, y - 1, z];
                            c.idX = x;
                            c.idY = y;
                            c.x = getClusterX(x, z);
                            c.y = getClusterY(y, z);
                            newClusters[x, y, z] = c;                            
                        }
                    }
                }
            }
            else
            {
                clustersBottom++;
                for (int z = 0; z < newClusters.GetLength(2); z++)
                {
                    int step = (int)Math.Pow(deepScale, z);
                    int xLimit = (int)Math.Ceiling((double)newClusters.GetLength(0) / step);
                    for (int x = 0; x < xLimit; x++)
                    {
                        int yMax = (int)Math.Ceiling((double)clusters.GetLength(1) / step);                        
                        newClusters[x, yMax, z] = new Cluster(getClusterX(x, z), getClusterY(yMax, z), clusterSize * Math.Pow(deepScale, z), x, yMax, z, layersCount);
                        for (int y = 0; y < yMax; y++)
                        {
                            Cluster c = clusters[x, y, z];
                            c.idX = x;
                            c.idY = y;
                            c.x = getClusterX(x, z);
                            c.y = getClusterY(y, z);
                            newClusters[x, y, z] = c;
                        }
                    }
                }
            }
            clusters = newClusters;
        }

        private void addClustersColumn(ref Cluster[,,] clusters, bool before)
        {
            Cluster[,,] newClusters = new Cluster[clusters.GetLength(0) + 1, clusters.GetLength(1), clusters.GetLength(2)];
            if (before)
            {
                clustersLeft++;
                for (int z = 0; z < newClusters.GetLength(2); z++)
                {
                    int step = (int)Math.Pow(deepScale, z);
                    int yLimit = (int)Math.Ceiling((double)newClusters.GetLength(1) / step);
                    for (int y = 0; y < yLimit; y++)
                    {
                        int xLimit = (int)Math.Ceiling((double)clusters.GetLength(0) / step) + 1;
                        newClusters[0, y, z] = new Cluster(getClusterX(0, z), getClusterY(y, z), clusterSize * Math.Pow(deepScale, z), 0, y, z, layersCount);
                        for (int x = 1; x < xLimit; x++)
                        {
                            Cluster c = clusters[x - 1, y, z];
                            c.idX = x;
                            c.idY = y;
                            c.x = getClusterX(x, z);
                            c.y = getClusterY(y, z);
                            newClusters[x, y, z] = c;
                        }
                    }
                }
            }
            else
            {
                clustersRight++;
                for (int z = 0; z < newClusters.GetLength(2); z++)
                {
                    int step = (int)Math.Pow(deepScale, z);
                    int yLimit = (int)Math.Ceiling((double)newClusters.GetLength(1) / step);
                    for (int y = 0; y < yLimit; y++)
                    {
                        int xMax = (int)Math.Ceiling((double)clusters.GetLength(0) / step);                        
                        newClusters[xMax, y, z] = new Cluster(getClusterX(xMax, z), getClusterY(y, z), clusterSize * Math.Pow(deepScale, z), xMax, y, z, layersCount);
                        for (int x = 0; x < xMax; x++)
                        {
                            Cluster c = clusters[x, y, z];
                            c.idX = x;
                            c.idY = y;
                            c.x = getClusterX(x, z);
                            c.y = getClusterY(y, z);
                            newClusters[x, y, z] = c;
                        }
                    }
                }
            }
            clusters = newClusters;
        }        
        
        private double getClusterX(int id, int deepnees)
        {
            int size = (int)(clusterSize * Math.Pow(deepScale, deepnees));
            int leftClusters = (int)Math.Ceiling(clustersLeft / Math.Pow(deepScale, deepnees));
            return size * (id - leftClusters);
        }

        private double getClusterY(int id, int deepnees)
        {
            int size = (int)(clusterSize * Math.Pow(deepScale, deepnees));
            int leftClusters = (int)Math.Ceiling(clustersTop / Math.Pow(deepScale, deepnees));
            return size * (id - leftClusters);
        }

        public void updatePoint(Pnt point, double interactRadius, int id)
        {
            DinamicPoint dpoint = pointsContainer.Get<DinamicPoint>()[id];
            updatePoint(dpoint, point.x - dpoint.x, point.y - dpoint.y, interactRadius);
        }

        private Cluster[] getClusters(double lpx, double x, double rpx, double tpy, double y, double bpy)
        {
            List<Cluster> output = new List<Cluster>();
            for (int z = 0; z < clusters.GetLength(2); z++)
            {
                lock (fillLocker)
                {
                    Cluster cl1 = getCluster(lpx, y, z);
                    Cluster cl2 = getCluster(rpx, y, z);
                    Cluster cl3 = getCluster(x, tpy, z);
                    Cluster cl4 = getCluster(x, bpy, z);
                    if (cl1 != null)
                        output.Add(cl1);
                    if (cl2 != null)
                        output.Add(cl2);
                    if (cl3 != null)
                        output.Add(cl3);
                    if (cl4 != null)
                        output.Add(cl4);
                }
            }
            return output.ToArray(); //Distinct()?
        }

        private Cluster[] getClusters(ManagedPoint point)
        {
            List<Cluster> output = new List<Cluster>();
            for (int z = 0; z < clusters.GetLength(2); z++)
            {
                lock (fillLocker)
                {
                    Cluster cl1 = getCluster(point.lpx, point.y, z);
                    Cluster cl2 = getCluster(point.rpx, point.y, z);
                    Cluster cl3 = getCluster(point.x, point.tpy, z);
                    Cluster cl4 = getCluster(point.x, point.bpy, z);
                    if (cl1 != null)
                        output.Add(cl1);
                    if (cl2 != null)
                        output.Add(cl2);
                    if (cl3 != null)
                        output.Add(cl3);
                    if (cl4 != null)
                        output.Add(cl4);
                }
            }
            return output.ToArray(); //Distinct()?
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
