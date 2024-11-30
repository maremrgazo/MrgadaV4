using Serilog;

public static partial class Mrgada
{
    public static DateTime DateTime;
    public static bool Collector1Connected;
    public static bool Collector2Connected;
    public static bool Collector3Connected;

    public class SyncVarServer : MrgadaTcpServer
    {
        public SyncVarServer(string serverName, string serverIp, int serverPort) : base(serverName, serverIp, serverPort)
        {
        }
        public void SyncVarServerThread()
        {
            while (Started)
            {

                Thread.Sleep(1000);

                DateTime = DateTime.Now;
                Broadcast(System.Text.Encoding.UTF8.GetBytes(DateTime.ToString()));
                Log.Information($"MrgadaSyncVar Server broadcast");
            }
        }
    }
    public class SyncVarClient : MrgadaTcpClient
    {
        public SyncVarClient(string serverName, string serverIp, int serverPort) : base(serverName, serverIp, serverPort)
        {
        }
        protected override void OnReceive(byte[] Buffer)
        {
            // On Client Receive set SyncVar values
            DateTime = DateTime.Now;
        }
    }
}