using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Mrgada
{
    public partial class c_Mrp6 : S7Collector
    {
        public c_dbDigialValvesSCADA dbDigialValvesSCADA;
        public c_dbAnalogSensorsSCADA dbAnalogSensorsSCADA;
        public c_Mrp6(string collectorName, int collectorPort, string plcIp, S7.Net.CpuType cpuType, short plcRack, short plcSlot) : base(collectorName, collectorPort, plcIp, cpuType, plcRack, plcSlot)
        {
            dbDigialValvesSCADA = new(52, 792, _s7CollectorClient, _s7Plc);
            dbAnalogSensorsSCADA = new(51, 2130, _s7CollectorClient, _s7Plc);

            AddS7db(dbDigialValvesSCADA);
            AddS7db(dbAnalogSensorsSCADA);
        }

    }
}
