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
    private static int _syncVarLogCount = 0;

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

                // Set the synced variables on server side
                DateTime = DateTime.Now;
                Random random = new Random();
                Collector1Connected = random.Next(2) == 1;
                Collector2Connected = random.Next(2) == 1;
                Collector3Connected = random.Next(2) == 1;

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

                if (_syncVarLogCount >= 10)
                {
                    _syncVarLogCount = 0;
                    Log.Information($"MrgadaSyncVar Server sent broadcast: {json}");
                }
                else { _syncVarLogCount++; }
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

            if (_syncVarLogCount >= 10)
            {
                _syncVarLogCount = 0;
                Log.Information($"MrgadaSyncVar Client recieved broadcast: {json}");
            }
            else { _syncVarLogCount++; }
        }
    }
}