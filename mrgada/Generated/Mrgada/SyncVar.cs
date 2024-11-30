using Serilog;
using System.Text.Json;
using System.Text;
using System;

public static partial class Mrgada
{
    public static class SyncedVars;
    public static DateTime DateTime;
    public static bool Collector1Connected;
    public static bool Collector2Connected;
    public static bool Collector3Connected;

    private class SyncedVariables
    {
        public DateTime DateTime { get; set; }
        public bool Collector1Connected { get; set; }
        public bool Collector2Connected { get; set; }
        public bool Collector3Connected { get; set; }
    }

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

                string json = JsonSerializer.Serialize
                (
                    new SyncedVariables
                    {
                        DateTime = DateTime.Now,
                        Collector1Connected = Collector1Connected,
                        Collector2Connected = Collector2Connected,
                        Collector3Connected = Collector3Connected
                    }
                );
                Broadcast(Encoding.UTF8.GetBytes(json));

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
            string json = Encoding.UTF8.GetString(Buffer);
            var syncVars = JsonSerializer.Deserialize<SyncedVariables>(json);
            if (syncVars != null)
            {
                Mrgada.DateTime = syncVars.DateTime;
                Mrgada.Collector1Connected = syncVars.Collector1Connected;
                Mrgada.Collector2Connected = syncVars.Collector2Connected;
                Mrgada.Collector3Connected = syncVars.Collector3Connected;
            }

            Log.Information($"MrgadaSyncVar Client recieved broadcast");
        }
    }
}