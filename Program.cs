
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .MinimumLevel.Debug()
        .CreateLogger();

        // Initialize Mrgada
        Mrgada.Init("192.168.64.107", 61100, Mrgada.e_MachineType.Server);

        // Instatiate S7Collector
        Mrgada.Mrp6 Mrp6 = new("Mrp6", 61101, "192.168.64.177", S7.Net.CpuType.S71500, 0, 1);
        // opccollector, rockwellcollector, etc   

        // Start Mrgada
        Mrgada.Start();

        //Mrgada.Stop();
 

        Console.ReadLine();
    }
}
