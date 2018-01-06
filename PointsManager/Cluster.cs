using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillionPointsManager
{
    public class Cluster
    {
        public DictionaryOfPointContainer container = new DictionaryOfPointContainer();
        public int pointsCount = 0;
        public Layer[] layers;
        public int idX, idY, idZ;
        public double x, y, size;
        public Cluster(double x, double y, double size, int idX, int idY, int idZ, int layersCount)
        {
            this.x = x;
            this.y = y;
            this.idX = idX;
            this.idY = idY;
            this.idZ = idZ;
            this.size = size;
            layers = new Layer[layersCount];
            double joinStep = size / layersCount;            
            for (int i = 0; i < layers.Length; i++)
                layers[i] = new Layer(i, joinStep * (i + 1));
        }

        private void addPointToLayer<PointType>(PointType point, int layer) where PointType : ManagedPoint
        {
            Layer currLayer = layers[layer];
            currLayer.addPoint(point);
        }

        public void addPoint<PointType>(PointType point) where PointType : ManagedPoint
        {
            ConcurrentDictionary<long, PointType> dictionary = container.Get<PointType>();
            if (dictionary.TryAdd(point.id, point))
            {
                foreach (Layer layer in layers)
                    layer.addPoint(point);
                //dictionary[point.id] = point;
                pointsCount++;
            }
        }

        
        public int getLayerId<PointType>(int maxCount) where PointType : ManagedPoint
        {
            for (int i = 0; i < layers.Length; i++)            
                if (layers[i].container.Get<PointType>().Count <= maxCount)
                    return i;            
            return layers.Length - 1;
        }

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

        public ManagedPoint[] getAllPointsAsArray()
        {
            int size = 0;
            foreach (IDictionary dict in container.Values)
                size += dict.Count;
            ManagedPoint[] pnts = new ManagedPoint[size];
            int index = 0;
            foreach (IDictionary dict in container.Values)
            {
                dict.Values.CopyTo(pnts, index);
                index += dict.Count;
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
