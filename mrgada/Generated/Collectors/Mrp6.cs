using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Mrgada
{
    public class Mrp6 : S7Collector
    {
        public Mrp6(string collectorName, int collectorPort, string plcIp) : base(collectorName, collectorPort, plcIp)
        {

        }
    }
}
