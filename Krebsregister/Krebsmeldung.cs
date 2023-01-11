using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krebsregister
{
    internal class Krebsmeldung
    {
        public string KrebsartCode { get; set; }
        public string Geschlecht { get; set; }
        public string Bundesland { get; set; }
        public int Anzahl { get; set; }
        public int Jahr { get; set; }
    }
}
