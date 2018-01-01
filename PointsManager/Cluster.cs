using MathAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class StoreCluster
    {
        /* ids:
         * 0 1 
         * 2 3
         */
        private static bool[,] knowTemplate = new bool[,] 
        {
            { true, false, true, false }, //left-right-top-bottom
            { false, true, true, false },
            { true, false, false, true},
            { false, true, false, true}
        };
        public StoreCluster[] subClusters = new StoreCluster[4];        
        public Layer[] layers;
        public double x, y, size;
        private bool leftKnown, rightKnown, topKnown, bottomKnown;

        public StoreCluster(
            double x, double y, double size, int layersCount,
            bool leftKnown = false, bool rightKnown = false, bool topKnown = false, bool bottomKnown = false)
        {
            this.leftKnown = leftKnown;
            this.rightKnown = rightKnown;
            this.topKnown = topKnown;
            this.bottomKnown = bottomKnown;
            this.x = x;
            this.y = y;
            this.size = size;
            layers = new Layer[layersCount];
            double joinStep = size / layersCount;
            for (int i = 0; i < layers.Length; i++)
                layers[i] = new Layer(i, joinStep * i);
        }        

        public void addPointDirectly(DinamicPoint point)
        {
            foreach (Layer layer in layers)
                layer.addPoint(point);
        }

        public void addSetDirectly(DinamicPointsSet set)
        {
            foreach (Layer layer in layers)
                layer.addSet(set);
        }

        public void addPointsDirectly(DinamicPoint[] points)
        {
            foreach (DinamicPoint p in points)
                addPointDirectly(p);
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

        public List<DinamicPointsSet> getPointSets(int maxCount)
        {
            for (int i = 0; i < layers.Length; i++)
                if (layers[i].sets.Count <= maxCount)
                    return layers[i].sets;
            return layers.Last().sets;
        }

        public List<DinamicPointsSet> getPointSets(Pnt leftTopView, Pnt rightBottomView, int maxCount)
        {
            //if all is okay
            for (int i = 0; i < layers.Length; i++)
                if (layers[i].sets.Count <= maxCount)
                    return layers[i].sets;

            //if there are too many clusters in view range
            List<DinamicPointsSet> sets = new List<DinamicPointsSet>();
            int pointsLeft = maxCount;
            for (int i = 0; i < subClusters.Length; i++)
            {
                StoreCluster c = subClusters[i];
                if (c == null)
                {
                    bool lk = knowTemplate[i, 0];
                    bool rk = knowTemplate[i, 1];
                    bool tk = knowTemplate[i, 2];
                    bool bk = knowTemplate[i, 3];
                    double x = this.x + size * (rk ? 1 : 0);
                    double y = this.y + size * (bk ? 1 : 0);
                    c = new StoreCluster(
                        x, y, size / 2, layers.Length,
                        lk, rk, tk, bk);

                    //now we need to add all stored pointSets in range
                    Layer zeroLayer = layers[0];
                    foreach (DinamicPointsSet set in zeroLayer.sets)
                        if (c.canAcceptPoint(set.x, set.y))
                            c.addSetDirectly(set);
                }
                List<DinamicPointsSet> newSets = c.getPointSets(leftTopView, rightBottomView, (pointsLeft / 4));
                sets.AddRange(newSets);
                pointsLeft -= newSets.Count;
            }

            return sets;
        }  
        
        public bool canAcceptPoint(
            double x, double y)
        {
            return 
                (leftKnown || x >= this.x) && (rightKnown || x <= this.x + size) && 
                (topKnown || y >= this.y) && (bottomKnown || y <= this.y + size);
        }
    }

    public class BaseCluster : StoreCluster
    {
        public HashSet<long> storedPoints = new HashSet<long>();
        public int idX, idY;
        
        public BaseCluster(double x, double y, double size, int idX, int idY, int layersCount)
            : base(x, y, size, layersCount)
        {            
            this.idX = idX;
            this.idY = idY;            
        }

        public void addPoint(DinamicPoint point)
        {
            if (!storedPoints.Contains(point.id))
            {
                foreach (Layer layer in layers)
                    layer.addPoint(point);
                foreach (StoreCluster c in subClusters)
                    if (c != null && c.canAcceptPoint(point.x, point.y))                        
                        c.addPointDirectly(point);

                storedPoints.Add(point.id);
            }
        }

        public void addPoints(DinamicPoint[] points)
        {
            foreach (DinamicPoint p in points)
                addPoint(p);
        }
    }
}
