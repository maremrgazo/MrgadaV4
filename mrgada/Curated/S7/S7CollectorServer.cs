﻿using S7.Net;
using Serilog;
using SerilogTimings;
using System.Diagnostics;
using System.Net.Sockets;

public static partial class Mrgada
{
    public class S7CollectorServer: MrgadaTcpServer
    {
        private List<byte> _broadcast = [];

        private S7.Net.Plc? _s7Plc;
        private List<Mrgada.S7db> _s7dbs;

        private Thread? t_collector;
        private bool b_collector;

        private string _collectorName;
        private int _collectorThreadMinInterval;
        private Stopwatch _collectorIntervalTimer = Stopwatch.StartNew();

        public bool CollectorConnected => _s7Plc?.IsConnected ?? false;
        public bool CollectorDisconnected => !CollectorConnected;

        public S7CollectorServer(List<Mrgada.S7db> s7dbs, string collectorName, string serverIp, int serverPort, S7.Net.Plc s7Plc, int collectorThreadMinInterval = 200) : base(collectorName, serverIp, serverPort)
        {
            _s7Plc = s7Plc;
            _collectorName = collectorName;
            _s7dbs = s7dbs;
            _collectorThreadMinInterval = collectorThreadMinInterval;
        }

        protected override void OnStart()
        {
            t_collector = new(CollectorThread);
            t_collector.IsBackground = true;
            t_collector.Start();

            b_collector = true;
        }
        protected override void OnStop()
        {
            b_collector = false;
            t_collector.Join();
        }

        protected override void OnConnect(TcpClient Client)
        {
            foreach (Mrgada.S7db s7db in _s7dbs)
            {
                s7db.OnClientConnect();
            }
        }

        public void CollectorThread() 
        {
            if (_s7Plc == null) throw new("Mrgada not Started!");
            while (b_collector)
            {
                _collectorIntervalTimer.Restart();
                if (CollectorConnected)
                {
                    
                    using (Operation.Time($"Reading bytes from S7 PLC: {_collectorName}"))
                    {
                        foreach (Mrgada.S7db s7db in _s7dbs)
                        {
                            s7db.SetBytes
                            (
                            _s7Plc.ReadBytes(S7.Net.DataType.DataBlock, s7db.Num, 0, s7db.Len)
                            );
                        }
                        foreach (Mrgada.S7db s7db in _s7dbs)
                        {
                            if (s7db.BroadcastFlag)
                            {
                                byte[] dbNum = BitConverter.GetBytes((short)s7db.Num);
                                byte[] chunkLength = BitConverter.GetBytes
                                    (
                                        (short)(sizeof(short) + sizeof(short) + s7db.Bytes.Length)
                                    );
                                _broadcast.AddRange(chunkLength);
                                _broadcast.AddRange(dbNum);
                                _broadcast.AddRange(s7db.Bytes);

                                s7db.ResetBroadcastFlag();
                            }
                        }
                        if (_broadcast.Count > 0) 
                        { 
                            // add broadcast length to start of list for partial transport checking
                            Int32 broadcastLength = sizeof(Int32) + _broadcast.Count;
                            _broadcast.InsertRange(0, BitConverter.GetBytes((Int32)broadcastLength));
                            // convert list to array
                            Broadcast(_broadcast.ToArray());
                            _broadcast.Clear();
                        }
                    }

                    _collectorIntervalTimer.Stop();
                    int remainingTime = (int)(_collectorThreadMinInterval - _collectorIntervalTimer.ElapsedMilliseconds);
                    if (remainingTime > 0) Thread.Sleep(remainingTime);
                }
                else
                {
                    try
                    {
                        if (_s7Plc == null) throw new("Mrgada not Started!");
                        _s7Plc.Open();
                    }
                    catch
                    {
                        Log.Information($"Can't connect to S7 PLC, trying again in 30 seconds: {_collectorName}");
                        Thread.Sleep(30000);
                    }
                }
            }
        }
    }
}