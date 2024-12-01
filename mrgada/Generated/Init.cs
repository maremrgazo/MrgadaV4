
public static partial class Mrgada
{
    public static Mrgada.c_Mrp6 Mrp6;
    static void InitCollectors()
    {
        Mrp6 = new("Mrp6", 61101, "192.168.64.177", S7.Net.CpuType.S71500, 0, 1);
    }
}