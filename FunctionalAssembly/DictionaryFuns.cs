using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalAssembly
{
    public static class DictionaryFuns
    {
        public static IList<TValue> Concat<TKey, TValue>(Dictionary<TKey, List<TValue>> dictionary)
        {
            int size = 0;            
            foreach (List<TValue> list in dictionary.Values)
                size += list.Count;
            TValue[] sets = new TValue[size];
            int index = 0;
            foreach (List<TValue> list in dictionary.Values)
            {
                list.CopyTo(sets, index);
                index += list.Count;
            }
            return sets;
        }
    }
}
