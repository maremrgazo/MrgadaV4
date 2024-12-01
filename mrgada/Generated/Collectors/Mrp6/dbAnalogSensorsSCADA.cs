
using static Mrgada.S7Collector;

public static partial class Mrgada
{
    public partial class Mrp6
    {
        public class dbAnalogSensorsSCADA: S7db
        {
            public dbAnalogSensorsSCADA(int num, int len) : base(num, len)
            {
            }


            public override void ParseCVs()
            {
                // Parse the bytes into CVs
            }
        }
    }
}