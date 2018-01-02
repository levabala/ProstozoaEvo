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
        Dictionary<Type, IDictionary> points = new Dictionary<Type, IDictionary>()
        {
            { typeof(StaticPoint), new Dictionary<long, StaticPoint>() },
            { typeof(DinamicPoint), new Dictionary<long, DinamicPoint>() },
        };
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

        private void addPointToLayer<PointType>(PointType point, int layer) where PointType: StaticPoint
        {
            Layer currLayer = layers[layer];
            bool nextAdd = currLayer.addPoint(point);
        }

        public void addPoint<PointType>(PointType point) where PointType: StaticPoint
        {
            Dictionary<long, PointType> dictionary = points[typeof(PointType)] as Dictionary<long, PointType>;
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

        public PointSet<PointType>[] getPointSets<PointType>(int maxCount) where PointType: StaticPoint
        {
            for (int i = 0; i < layers.Length; i++)
            {
                IList sets = layers[i].pointSets[typeof(PointType)];
                if (sets.Count <= maxCount)
                    return (sets as List<PointSet<PointType>>).ToArray();
            }
            return (layers.Last().pointSets[typeof(PointType)] as List<PointSet<PointType>>).ToArray();
        }
    }
}
