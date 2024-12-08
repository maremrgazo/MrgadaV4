
using static Mrgada.S7Collector;

public static partial class Mrgada
{
    public partial class c_Mrp6
    {
        public class c_dbDigialValvesSCADA: S7db
        {
            public udtSCADAAnalogSensor FT_6IN_702;
            public c_dbDigialValvesSCADA(int num, int len) : base(num, len)
            {
            }

            public override void ParseCVs()
            {
            }
        }
    }
}