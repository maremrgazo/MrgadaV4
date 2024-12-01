using Serilog;
using System.Text.Json;
using System.Text;
using System;

public static partial class Mrgada
{
    public static class SyncedVars;
    public static DateTime DateTime;
    public static bool Mrp6CollectorStatus;

    private class SyncedVariables
    {
        public DateTime DateTime { get; set; }
        public bool Mrp6CollectorStatus { get; set; }
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
                Mrp6CollectorStatus = true;

                string json = JsonSerializer.Serialize
                (
                    new SyncedVariables
                    {
                        DateTime = DateTime.Now,
                        Mrp6CollectorStatus = Mrp6CollectorStatus
                    }
                );
                List<byte> bytes = [];
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                byte[] jsonBytesLength = BitConverter.GetBytes((Int32)jsonBytes.Length);
                bytes.AddRange(jsonBytesLength);
                bytes.AddRange(jsonBytes);

                Broadcast(bytes.ToArray());

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
            Int32 jsonBytesLength = BitConverter.ToInt32(Buffer, 0);
            byte[] jsonBytes = new byte[jsonBytesLength];
            Array.Copy(jsonBytes, sizeof(Int32), jsonBytes, 0, jsonBytesLength);    

            string json = Encoding.UTF8.GetString(jsonBytes);
            var syncVars = JsonSerializer.Deserialize<SyncedVariables>(json);
            if (syncVars != null)
            {
                Mrgada.DateTime = syncVars.DateTime;
                Mrgada.Mrp6CollectorStatus = syncVars.Mrp6CollectorStatus;
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