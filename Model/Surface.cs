using MathAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Surface
    {
        public Dictionary<SourceType, List<long>> sourcePointsTyped = new Dictionary<SourceType, List<long>>();
        public Dictionary<long, SourcePoint> sourcePoints = new Dictionary<long, SourcePoint>();
        double leftB, topB, rightB, bottomB;        

        public Surface()
        {
            var possibleTypes = Enum.GetValues(typeof(SourceType));
            foreach (SourceType stype in possibleTypes)
                sourcePointsTyped[stype] = new List<long>();
        }

        public void addSourcePoint(SourcePoint spoint)
        {            
            sourcePointsTyped[spoint.sourceType].Add(spoint.id);
            sourcePoints.Add(spoint.id, spoint);            
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

            foreach (SourcePoint sp in sourcePoints.Values)
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
            foreach (long id in sourcePointsTyped[stype])
            {
                SourcePoint sp = sourcePoints[id];
                double dist = Vector.GetLength(point, sp.location);
                if (dist == 0)
                    continue;
                double coeff = 1 / dist;
                effect += coeff * sp.strength;
            }
            return effect;
        }

        public Dictionary<SourceType, double> getEffectAtPoint(Pnt point, SourceType[] stypes)
        {
            Dictionary<SourceType, double> effects = new Dictionary<SourceType, double>();
            foreach (SourceType stype in stypes)
                effects[stype] = 0;
            foreach (SourcePoint sp in sourcePoints.Values)
            {
                if (!effects.ContainsKey(sp.sourceType))
                    continue;

                double dist = Vector.GetLength(point, sp.location);
                if (dist == 0)
                    continue;
                double coeff = 1 / dist;
                effects[sp.sourceType] += coeff * sp.strength;
            }
            return effects;
        }

        public Pnt getRandomPoint(Random rnd, int maxDistance)
        {
            if (sourcePoints.Count == 0)
                return new Pnt(0, 0);

            int x = rnd.Next((int)leftB - maxDistance, (int)rightB + maxDistance);
            int y = rnd.Next((int)topB - maxDistance, (int)bottomB + maxDistance);
            return new Pnt(x, y);
        }

        public Pnt getRandomPointFromLast(Random rnd, int maxDistance)
        {
            if (sourcePoints.Count == 0)
                return new Pnt(0, 0);

            SourcePoint last = sourcePoints.Values.Last();
            int x = rnd.Next((int)last.location.x - maxDistance, (int)last.location.x + maxDistance);
            int y = rnd.Next((int)last.location.y - maxDistance, (int)last.location.y + maxDistance);
            return new Pnt(x, y);
        }
    }

    public class Range
    {
        public double from, to;
        public Range()
        {
            from = Int32.MinValue;
            to = Int32.MaxValue;
        }

        public Range(double from)
        {
            this.from = from;
            to = Int32.MaxValue;
        }

        public Range(double from, double to)
        {
            this.from = from;
            this.to = to;
        }

        public double toRange(double value)
        {
            if (value > to)
                return to;
            if (value < from)
                return from;
            return value;
        }
    }

    public struct SourcePoint
    {
        public static Dictionary<SourceType, Range> rangesStrength = new Dictionary<SourceType, Range>()
        {
            { SourceType.Fertility, new Range(0.01, 0.1) },//new Range(0.001, 0.01) },
            { SourceType.Toxicity, new Range(0, 1) },
            { SourceType.Viscosity, new Range(0, 1) },
            { SourceType.Fire, new Range(0, 1) },
            { SourceType.Grass, new Range(0, 1) },
            { SourceType.Ocean, new Range(0, 1) },
        };
        public static Dictionary<SourceType, Range> rangesDistance = new Dictionary<SourceType, Range>()
        {
            { SourceType.Fertility, new Range(200, 500) },
            { SourceType.Toxicity, new Range(200, 500) },
            { SourceType.Viscosity, new Range(200, 500) },
            { SourceType.Fire, new Range(200, 600) },
            { SourceType.Grass, new Range(200, 600) },
            { SourceType.Ocean, new Range(200, 600) },
        };

        public Pnt location;
        public SourceType sourceType;
        public double strength, distance;
        public long id;

        public SourcePoint(Pnt location, SourceType sourceType, double strength, double distance, long id = 0)
        {
            this.id = id;
            this.location = location;
            this.sourceType = sourceType;
            this.strength = rangesStrength[sourceType].toRange(strength);
            this.distance = rangesStrength[sourceType].toRange(distance);
        }

        public SourcePoint(Pnt location, SourceType sourceType, Random rnd, long id = 0)
        {
            this.id = id;
            this.location = location;
            this.sourceType = sourceType;
            Range rangeStrength = rangesStrength[sourceType];
            Range rangeDistance= rangesDistance[sourceType];
            strength = rnd.NextDouble() * (rangeStrength.to - rangeStrength.from) + rangeStrength.from;
            distance = rnd.NextDouble() * (rangeDistance.to - rangeDistance.from) + rangeDistance.from;
        }
    }

    public enum SourceType
    {
        Toxicity,
        Fertility,
        Viscosity,
        Fire,
        Grass,
        Ocean
    }
}
