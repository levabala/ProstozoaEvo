using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Surface
    {
        public List<SourcePoint> sourcePoints = new List<SourcePoint>();
        double leftB, topB, rightB, bottomB;

        public Surface()
        {
            
        }

        public void addSourcePoint(SourcePoint spoint)
        {
            sourcePoints.Add(spoint);
            if (sourcePoints.Count == 1)
            {
                leftB = rightB = spoint.location.x;
                topB = bottomB = spoint.location.y;
                return;
            }

            if (spoint.location.x < leftB)
                leftB = spoint.location.x;
            if (spoint.location.x > rightB)
                rightB = spoint.location.x;
            if (spoint.location.y < topB)
                topB = spoint.location.y;
            if (spoint.location.y > bottomB)
                bottomB = spoint.location.y;
        }

        public Dictionary<SourceType, double> getEffectAtPoint(Pnt point)
        {
            Dictionary<SourceType, double> output = new Dictionary<SourceType, double>();
            var possibleTypes = Enum.GetValues(typeof(SourceType));
            foreach (SourceType stype in possibleTypes)
                output[stype] = 0;

            foreach (SourcePoint sp in sourcePoints)
            {
                double dist = Vector.GetLength(point, sp.location);
                if (dist == 0)
                    continue;
                double coeff = 1 / dist;
                output[sp.sourceType] += coeff * sp.strength;
            }
            return output;
        }

        public double getEffectAtPoint(Pnt point, SourceType stype)
        {
            double effect = 0;
            foreach (SourcePoint sp in sourcePoints)
            {
                if (sp.sourceType != stype)
                    continue;
                double dist = Vector.GetLength(point, sp.location);
                if (dist == 0)
                    continue;
                double coeff = 1 / dist;
                effect += coeff * sp.strength;
            }
            return effect;
        }

        public Pnt getRandomPoint(Random rnd, int maxDistance)
        {
            if (sourcePoints.Count == 0)
                return new Pnt(0, 0);

            int x = rnd.Next((int)leftB - maxDistance, (int)rightB + maxDistance);
            int y = rnd.Next((int)topB - maxDistance, (int)bottomB + maxDistance);
            return new Pnt(x, y);
        }
    }

    public struct SourcePoint
    {
        public Pnt location;
        public SourceType sourceType;
        public double strength;

        public SourcePoint(Pnt location, SourceType sourceType, double strength)
        {
            this.location = location;
            this.sourceType = sourceType;
            this.strength = strength;
        }
    }

    public enum SourceType
    {
        Toxicity,
        Fertility,
        Viscosity
    }
}
