using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class Cluster
    {
        public Dictionary<long, DinamicPoint> points = new Dictionary<long, DinamicPoint>();
        public Layer[] layers;
        public int idX, idY, idZ;
        public double x, y, size;
        public Cluster(double x, double y, double size, int idX, int idY, int idZ, int layersCount, int deepStep)
        {
            this.x = x;
            this.y = y;
            this.idX = idX;
            this.idY = idY;
            this.size = size;
            layers = new Layer[layersCount];
            double joinStep = (size / layersCount) + deepStep * idZ;
            //for (int i = layersCount - 1; i >= 0; i--)
            for (int i = 0; i < layers.Length; i++)
                layers[i] = new Layer(i, joinStep * i);
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
}
