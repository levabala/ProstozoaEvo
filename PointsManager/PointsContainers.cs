using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillionPointsManager
{
    public class PointSetsContainer
    {
        public ICollection<IList> Values
        {
            get
            {
                return pointSets.Values;
            }
        }
        public ConcurrentDictionary<Type, IList> pointSets = 
            new ConcurrentDictionary<Type, IList>(new List<KeyValuePair<Type, IList>>()
        {
            new KeyValuePair<Type, IList>(typeof(StaticPoint), new List<PointSet>()),
            new KeyValuePair<Type, IList>(typeof(DinamicPoint), new List<PointSet>())
        });       
        public ConcurrentDictionary<Type, object> listLockers =
            new ConcurrentDictionary<Type, object>(new List<KeyValuePair<Type, object>>()
        {
            new KeyValuePair<Type, object>(typeof(StaticPoint), new object()),
            new KeyValuePair<Type, object>(typeof(DinamicPoint), new object())
        });        

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
        public ICollection<IList> Values
        {
            get
            {
                return pointSets.Values;
            }
        }

        public ConcurrentDictionary<Type, IList> pointSets =
            new ConcurrentDictionary<Type, IList>(new List<KeyValuePair<Type, IList>>()
        {
            new KeyValuePair<Type, IList>(typeof(StaticPoint), new List<StaticPoint>()),
            new KeyValuePair<Type, IList>(typeof(DinamicPoint), new List<DinamicPoint>())
        });

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
        public ICollection<IDictionary> Values
        {
            get
            {
                return dictionaries.Values;
            }
        }
        ConcurrentDictionary<Type, IDictionary> dictionaries = new ConcurrentDictionary<Type, IDictionary>();        

        public DictionaryOfPointContainer()
        {
            dictionaries.TryAdd(typeof(StaticPoint), new ConcurrentDictionary<long, StaticPoint>());
            dictionaries.TryAdd(typeof(DinamicPoint), new ConcurrentDictionary<long, DinamicPoint>());            
        }

        public ConcurrentDictionary<long, PointType> Get<PointType>() where PointType : ManagedPoint
        {
            return dictionaries[typeof(PointType)] as ConcurrentDictionary<long, PointType>;
        }
    }

    public interface ICountable
    {
        ICollection Values { get; }
        int Count { get; }
    }
}
