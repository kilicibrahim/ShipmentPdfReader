using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipmentPdfReader.Helpers
{
    public static class CollectionExtensions
    {
        public static void RemoveAll<T>(this ICollection<T> collection)
        {
            var items = collection.ToList();
            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }
    }
}
