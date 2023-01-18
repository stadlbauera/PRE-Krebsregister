using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krebsregister
{
    public class Krebsmeldung
    {
        public string Krebsart { get; set; }
        public string ICD10Code { get; set; }
        public string Geschlecht { get; set; }
        public string Bundesland { get; set; }
        public int Anzahl { get; set; }
        public int Jahr { get; set; }
    }
}
