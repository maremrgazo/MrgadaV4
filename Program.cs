
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .MinimumLevel.Debug()
        .CreateLogger();

        //Mrgada.MrgadaTcpClient mrgadaTcpClient = new Mrgada.MrgadaTcpClient("MRP6", "192.168.64.107", 61102);
        //Mrgada.MrgadaTcpServer mrgadaTcpServer = new Mrgada.MrgadaTcpServer("MRP6", "192.168.64.107", 61102);

        // Initialize Mrgada
        Mrgada.Init("192.168.64.107", 61100, Mrgada.e_MachineType.Server);

        // Instatiate S7Collector
        Mrgada.Mrp6 Mrp6 = new("Mrp6", 61101, "192.168.64.177");
        // opccollector, rockwellcollector, etc   

        // Start Mrgada
        Mrgada.Start();

        Console.ReadLine();
    }
}

////    Thread.Sleep(9999);

////    mrgadaTcpClient.Stop();
////}


//mrgadaTcpServer.Start();

//while(mrgadaTcpServer.Stopped) {}

//mrgadaTcpClient.Start();

//Thread.Sleep(1000);

//byte[] buffer = new byte[1024];
//mrgadaTcpServer.Broadcast(buffer);

//Thread.Sleep(1000);

//buffer = new byte[1023];
//mrgadaTcpClient.Send(buffer);

//Thread.Sleep(1000);

//mrgadaTcpClient.Stop();

//mrgadaTcpServer.Stop();

//Console.WriteLine("end");

//Console.ReadLine();