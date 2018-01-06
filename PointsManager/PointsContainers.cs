using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsManager
{
    public class PointSetsContainer
    {
        public Dictionary<Type, IList>.ValueCollection Values
        {
            get
            {
                return pointSets.Values;
            }
        }
        public Dictionary<Type, IList> pointSets = new Dictionary<Type, IList>()
        {
            { typeof(StaticPoint), new List<PointSet>() },
            { typeof(DinamicPoint), new List<PointSet>() },
        };
        public Dictionary<Type, object> listLockers = new Dictionary<Type, object>()
        {
            { typeof(StaticPoint), new object() },
            { typeof(DinamicPoint), new object() },
        };

        public PointSetsContainer()
        {
            
        }

        public List<PointSet> Get<PointType>() where PointType : ManagedPoint
        {
            return pointSets[typeof(PointType)] as List<PointSet>;
        }

        public object GetLocker<PointType>() where PointType : ManagedPoint
        {
            return listLockers[typeof(PointType)];
        }
    }

    public class PointContainer
    {
        public Dictionary<Type, IList>.ValueCollection Values
        {
            get
            {
                return pointSets.Values;
            }
        }
        public Dictionary<Type, IList> pointSets = new Dictionary<Type, IList>()
        {
            { typeof(StaticPoint), new List<StaticPoint>() },
            { typeof(DinamicPoint), new List<DinamicPoint>() },
        };

        public PointContainer()
        {

        }

        public List<PointType> Get<PointType>() where PointType : ManagedPoint
        {
            return pointSets[typeof(PointType)] as List<PointType>;
        }
    }
    public class DictionaryOfPointContainer
    {
        public Dictionary<Type, IDictionary>.ValueCollection Values
        {
            get
            {
                return dictionaries.Values;
            }
        }
        Dictionary<Type, IDictionary> dictionaries = new Dictionary<Type, IDictionary>()
        {
            { typeof(StaticPoint), new Dictionary<long, StaticPoint>() },
            { typeof(DinamicPoint), new Dictionary<long, DinamicPoint>() },
        };

        public DictionaryOfPointContainer()
        {

        }

        public Dictionary<long, PointType> Get<PointType>() where PointType : ManagedPoint
        {
            return dictionaries[typeof(PointType)] as Dictionary<long, PointType>;
        }
    }

}
