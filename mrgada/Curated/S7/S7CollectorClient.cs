using S7.Net;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Mrgada;
using static Mrgada.c_Mrp6;

public static partial class Mrgada
{
    public class S7CollectorClient: MrgadaTcpClient
    {
        private string _collectorName;
        private List<Mrgada.S7db> _s7dbs;

        private List<byte> _send = [];
        private Thread? t_send;
        private bool b_send;
        private object o_sendLock = new();
        private int i_sendTimeout = 200;

        public void AddToSendQueue(byte[] data)
        {
            lock (o_sendLock)
            {
                _send.AddRange(data);
            }
        }

        public S7CollectorClient(List<Mrgada.S7db> s7dbs, string collectorName, string serverIp, int serverPort, int connectHandlerTimeout = 3000, int receiveBroadcastTimeout = 200) : base(collectorName, serverIp, serverPort, connectHandlerTimeout, receiveBroadcastTimeout)
        {
            _collectorName = collectorName;
            _s7dbs = s7dbs;
        }
        protected override void OnStart()
        {
            t_send = new(SendThread);
            t_send.IsBackground = true;
            t_send.Start();

            b_send = true;
        }
        protected override void OnStop()
        {
            b_send = false;
            t_send.Join();
        }
        private void SendThread()
        {
            while(b_send)
            {
                if (Connected)
                {
                    if (_send.Count > 0)
                    {
                        lock (o_sendLock)
                        {
                            Int32 chunkLength = sizeof(Int32) + _send.Count;
                            _send.InsertRange(0, BitConverter.GetBytes((Int32)chunkLength));
                            Send(_send.ToArray());
                            _send.Clear();
                        }
                    }
                    else Thread.Sleep(i_sendTimeout);
                }
                else
                {
                    Thread.Sleep(i_connectHandlerTimeout);
                }
            }
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