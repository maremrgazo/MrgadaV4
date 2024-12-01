
using static Mrgada.S7Collector;

public static partial class Mrgada
{
    public partial class Mrp6
    {
        public class dbDigialValvesSCADA: S7db
        {
            public udtSCADAAnalogSensor FT_6IN_702;
            public dbDigialValvesSCADA(int num, int len) : base(num, len)
            {
            }

            public override void ParseCVs()
            {
                // Parse the bytes into CVs
            }
        }
    }
}