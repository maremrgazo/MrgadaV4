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
            dbDigialValves dbDigialValves = new(52, 792);
            dbDigialValves dbDigialValves1 = new(52, 792);
            dbDigialValves dbDigialValves2 = new(52, 792);
            dbDigialValves dbDigialValves3 = new(52, 792);
            dbDigialValves dbDigialValves4 = new(52, 792);

            AddS7db(dbDigialValves);
            AddS7db(dbDigialValves1);
            AddS7db(dbDigialValves2);
            AddS7db(dbDigialValves3);
            AddS7db(dbDigialValves4);

            //for (int i = 0; i < 99; i++)
            //{
            //    AddS7db(dbDigialValves);
            //}
        }
    }
}
