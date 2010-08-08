using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Validator
{
    static class ObservableCollectionHelper
    {
        public static void AddRange<T>(this ObservableCollection<T> oc, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                oc.Add(item);
            }
        }
    }
}
