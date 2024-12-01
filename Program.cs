
using Microsoft.Extensions.Configuration;
using Serilog;
using static Mrgada;

public class Program
{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.

    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .MinimumLevel.Debug()
        .CreateLogger();

        var mrgadaConfig = configuration.GetSection("MrgadaConfig");
        if (!mrgadaConfig.Exists()) throw new Exception("MrgadaConfig section does not exist in appsettings.json.");
        string MrgadaServerIp = mrgadaConfig["ServerIp"];
        string s_MachineType = mrgadaConfig["MachineType"];

        Mrgada.e_MachineType MachineType = mrgadaConfig["MachineType"] == "Server" ? Mrgada.e_MachineType.Server : Mrgada.e_MachineType.Client;

        // Initialize Mrgada
        Mrgada.Init(MrgadaServerIp, 61100, MachineType);

        // opccollector, rockwellcollector, etc   

        // Start Mrgada
        Mrgada.Start();

        Thread.Sleep(3000);

        if (Mrgada.MachineType == Mrgada.e_MachineType.Server) Mrgada.Mrp6.dbAnalogSensorsSCADA.Spare_SC_5B2_112.ManValueEGU.CV = 100;

        //Mrgada.Stop();


        Thread.Sleep(Timeout.Infinite);
    }
}
