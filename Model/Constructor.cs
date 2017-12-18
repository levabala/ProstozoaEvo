using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Constructor
    {
        public static double MUTATIVE = 0.1;
        public static double NEW_MUTATIVE = 0.5;

        //standart constructor
        public Dictionary<ParamName, Param> fields = new Dictionary<ParamName, Param>()
        {
            { ParamName.BirthEnergy, new Param(10, 1, 5) },
            { ParamName.BirthRadius, new Param(8, 1, 3) },            
            { ParamName.EnergyCapacity, new Param(100, 1.2, 50) },
            { ParamName.AccLimit, new Param(5, 2, 3) },
            { ParamName.CooldownEat, new Param(0.5, 1.5, 0.3) },
            { ParamName.CooldownLove, new Param(3, 3, 2) },
            { ParamName.Fire, new Param(0.5, 0, 0.5, true) },
            { ParamName.Grass, new Param(0.5, 0, 0.5, true) },
            { ParamName.Ocean, new Param(0.5, 0, 0.5, true) },
            { ParamName.ViewDepth, new Param(10, 0.5, 5) },
            { ParamName.ViewWidth, new Param(4, 0.3, 3) },
            { ParamName.Fearfulness, new Param(0.5, 0.1, 0.5, true) },
        };
        /*
        double
            birthEnergy, energyCapacity, accLimit, cooldownEat,
            cooldownLove, fire, grass, ocear,
            viewDepth, viewWidth, fearfulness;*/

        public Constructor(Random rnd)
        {
            mutateAll(rnd);
        }

        public Constructor(
            Random rnd, Constructor constr1, Constructor constr2, 
            double coeff1 = 0.5, double coeff2 = 0.5, double addMutative = 0)
        {
            foreach (ParamName name in fields.Keys)
            {
                fields[name].value = constr1.fields[name].value * coeff1 + constr2.fields[name].value * coeff2;
                mutate(rnd, fields[name], fields[name].mutative + addMutative);
            }                
        }

        public void mutateAll(Random rnd)
        {            
            foreach (Param param in fields.Values)            
                mutate(rnd, param);            
        }

        public Param mutate(Random rnd, Param param)
        {
            return mutate(rnd, param, param.mutative);
        }

        public Param mutate(Random rnd, Param param, double mutative)
        {
            double val = param.value;
            val += rnd.NextDouble() * 2 * mutative - mutative;
            if (param.zerooneLimited)
                if (val > 1)
                    val = 1;
                else if (val < 0)
                    val = 0;
            param.value = val;

            return param;
        }

        public double getParamValue(ParamName name)
        {
            return fields[name].value;
        }

        public double getCost()
        {
            double cost = 0;
            foreach (Param p in fields.Values)
                cost += p.value * p.cost;
            return cost;
        }
    }

    public class Param
    {
        public double value, cost, mutative;
        public bool zerooneLimited;
        public Param(double value, double cost, double mutative, bool zerooneLimited = false)
        {
            this.value = value;
            this.cost = cost;
            this.mutative = mutative;
            this.zerooneLimited = zerooneLimited;
        }
    }

    public enum ParamName
    {
        BirthEnergy,
        BirthRadius,        
        EnergyCapacity,
        Fire,
        Grass,
        Ocean,
        CooldownEat,
        CooldownLove,
        ViewDepth,
        ViewWidth,
        AccLimit,
        Fearfulness
    }
}
