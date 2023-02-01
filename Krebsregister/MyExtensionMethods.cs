using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krebsregister
{
    public static class MyExtensionMethods
    {
        public static List<int> MySum(this List<Krebsmeldung> l, List<int> jahre)
        {
            var list = new List<int>();
            foreach (var jahr in jahre)
            {
                int counter = 0;
                foreach (var meldung in l)
                {
                    if(meldung.Jahr == jahr)
                    {
                        counter += meldung.Anzahl;
                    }
                }
                list.Add(counter);
            }
            return list;
        }
    }
}
