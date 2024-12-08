﻿public static partial class Mrgada
{
    public static partial class S7Historian
    {
        public static c_mrp6 mrp6 = new c_mrp6();
        public class c_mrp6 : HistorianDb
        {
            public Tag<float> PT_6B3_103;
            public c_mrp6() : base("mrp6")
            {
                PT_6B3_103 = new Tag<float>(Mrp6.dbAnalogSensorsSCADA.PT_6B3_103.ValueEgu, this, "PT_6B3_103", "bar");
            }
        }
    }
}