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

        public static List<Krebsmeldung> MyWhere(this List<Krebsmeldung> alleKrebsmeldungen, List<string> gewünschteICD10s)
        {
            List<Krebsmeldung> filtered = new List<Krebsmeldung>();
            foreach (Krebsmeldung aktuelleKrebsmeldung in alleKrebsmeldungen)
            {
                foreach(string aktuellerICD10 in gewünschteICD10s)
                {
                    if (aktuelleKrebsmeldung.ICD10Code.Equals(aktuellerICD10)) filtered.Add(aktuelleKrebsmeldung);
                }
                
            }
            return filtered;
        }
    }
}
