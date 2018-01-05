using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class Cluster
    {
        public DictionaryOfPointContainer container = new DictionaryOfPointContainer();
        public int pointsCount = 0;
        public Layer[] layers;
        public int idX, idY;
        public double x, y, size;
        public Cluster(double x, double y, double size, int idX, int idY, int layersCount)
        {
            this.x = x;
            this.y = y;
            this.idX = idX;
            this.idY = idY;
            this.size = size;
            layers = new Layer[layersCount];
            double joinStep = size / layersCount;            
            for (int i = 0; i < layers.Length; i++)
                layers[i] = new Layer(i, joinStep * i);
        }

        private void addPointToLayer<PointType>(PointType point, int layer) where PointType : ManagedPoint
        {
            Layer currLayer = layers[layer];
            currLayer.addPoint(point);
        }

        public void addPoint<PointType>(PointType point) where PointType : ManagedPoint
        {
            Dictionary<long, PointType> dictionary = container.Get<PointType>();
            if (!dictionary.ContainsKey(point.id))
            {
                foreach (Layer layer in layers)
                    layer.addPoint(point);
                dictionary[point.id] = point;
                pointsCount++;
            }
        }

        /*
        public int getLayerId(int maxPointsCount)
        {
            for (int i = 0; i < layers.Length; i++)
                if (layers[i].dinamicSets.Count + layers[i].staticSets.Count <= maxPointsCount)
                    return i;
            return layers.Length - 1;
        }*/

        //TODO
        public void removePoint(long id)
        {
            //here removing from layers, common dictionary..
        }

        public PointSet[] getPointSets<PointType>(int maxCount) where PointType : ManagedPoint
        {
            for (int i = 0; i < layers.Length; i++)
            {
                IList sets = layers[i].container.Get<PointType>();
                if (sets.Count <= maxCount)
                    return (sets as List<PointSet>).ToArray();
            }
            return layers.Last().container.Get<PointType>().ToArray();
        }

        public StaticPoint[] getAllPointsAsArray()
        {
            int size = 0;
            foreach (List<StaticPoint> list in container.Values)
                size += list.Count;
            StaticPoint[] pnts = new StaticPoint[size];
            int index = 0;
            foreach (List<StaticPoint> list in container.Values)
            {
                list.CopyTo(pnts, index);
                index += list.Count;
            }
            return pnts;
        }

        public List<StaticPoint> getAllPoints()
        {
            List<StaticPoint> list = new List<StaticPoint>();
            foreach (List<StaticPoint> l in container.Values)
                list.AddRange(l);
            return list;
        }

        public bool containsPoint(long id)
        {
            foreach (List<StaticPoint> l in container.Values)
                foreach (StaticPoint p in l)
                    if (p.id == id)
                        return true;
            return false;
        }
    }
}
