
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

Mrgada.MrgadaTcpClient mrgadaTcpClient = new Mrgada.MrgadaTcpClient("MRP6", "192.168.64.107", 61102);

mrgadaTcpClient.Start();

Console.ReadLine();