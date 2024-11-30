using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Mrgada
{
    public partial class Mrp6 : S7Collector
    {
        public Mrp6(string collectorName, int collectorPort, string plcIp, S7.Net.CpuType cpuType, short plcRack, short plcSlot) : base(collectorName, collectorPort, plcIp, cpuType, plcRack, plcSlot)
        {
            dbDigialValves dbDigialValves = new(52, 200);
            AddS7db(dbDigialValves);
        }
    }
}
