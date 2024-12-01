﻿using Serilog;
using System.Collections;
using System.Text;
using static Mrgada;
using static Mrgada.Mrp6;

public static partial class Mrgada
{
    public class S7CollectorClient: MrgadaTcpClient
    {
        private string _collectorName;
        private List<Mrgada.S7db> _s7dbs;
        public S7CollectorClient(List<Mrgada.S7db> s7dbs, string collectorName, string serverIp, int serverPort, int connectHandlerTimeout = 3000, int receiveBroadcastTimeout = 200) : base(collectorName, serverIp, serverPort, connectHandlerTimeout, receiveBroadcastTimeout)
        {
            _collectorName = collectorName;
            _s7dbs = s7dbs;
        }

        protected override void OnReceive(byte[] data)
        {
            Int32 broadcastLength = BitConverter.ToInt32(data, 0);
            bool isPartial = data.Length != broadcastLength;

            Log.Information($"Client Recieved Broadcast, isPartial ({isPartial}) len ({data.Length}) from S7 Collector: {_collectorName}");

            int i = sizeof(Int32); // skip broadcastLength
            while (i < data.Length)
            {
                short i16_chunkLength = BitConverter.ToInt16(data, i);
                short i16_dbNumber = BitConverter.ToInt16(data, i + sizeof(Int16));
                byte[] ba_dbBytes = new byte[i16_chunkLength - (sizeof(Int16) + sizeof(Int16))];
                Buffer.BlockCopy(data, i + sizeof(Int16) + sizeof(Int16), ba_dbBytes, 0, ba_dbBytes.Length);

                foreach(Mrgada.S7db s7db in _s7dbs)
                {
                    if (s7db.Num == i16_dbNumber)
                    {
                        s7db.SetBytes(ba_dbBytes);
                        s7db.ParseCVs();
                        Log.Information($"  Client chunk, db ({i16_dbNumber}), len ({ba_dbBytes.Length}) from S7 Collector: {_collectorName}");

                        // Get last 10 bytes of db and log
                        int lengthToTake = Math.Min(10, ba_dbBytes.Length);
                        byte[] lastBytes = new byte[lengthToTake];
                        Array.Copy(ba_dbBytes, ba_dbBytes.Length - lengthToTake, lastBytes, 0, lengthToTake);
                        string s_lastBytes = BitConverter.ToString(lastBytes).Replace("-", " "); ;
                        Log.Information($"      Last ~ 10 bytes are, {s_lastBytes} ");

                        break;
                    }
                }
                i += i16_chunkLength;
            }
        }
    }
}