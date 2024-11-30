using S7.Net;
using Serilog;
using SerilogTimings;
using System.Diagnostics;

public static partial class Mrgada
{
    public class S7CollectorServer: MrgadaTcpServer
    {
        private S7.Net.Plc? _s7Plc;
        private S7.Net.CpuType _cpuType;
        private string _plcIp;
        private short _plcRack;
        private short _plcSlot;
        private List<Mrgada.S7Collector.S7db> _s7dbs;

        private Thread? t_collector;
        private bool b_collector;
        private string _collectorName;
        private int _collectorThreadMinInterval;
        private Stopwatch _collectorIntervalTimer = Stopwatch.StartNew();

        public bool CollectorConnected => _s7Plc?.IsConnected ?? false;
        public bool CollectorDisconnected => !CollectorConnected;

        public S7CollectorServer(List<Mrgada.S7Collector.S7db> s7dbs, string collectorName, string serverIp, int serverPort, S7.Net.CpuType cpuType, string plcIp, short plcRack, short plcSlot, int collectorThreadMinInterval = 200) : base(collectorName, serverIp, serverPort)
        {
            _cpuType = cpuType;
            _plcIp = plcIp;
            _collectorName = collectorName;
            _s7dbs = s7dbs;
            _collectorThreadMinInterval = collectorThreadMinInterval;
        }

        protected override void OnStart()
        {
            _s7Plc = new(_cpuType, _plcIp, _plcRack, _plcSlot);

            t_collector = new(CollectorThread);
            t_collector.IsBackground = true;
            t_collector.Start();

            b_collector = true;
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
                        foreach (Mrgada.S7Collector.S7db s7db in _s7dbs)
                        {
                            s7db.SetBytes
                            (
                            _s7Plc.ReadBytes(S7.Net.DataType.DataBlock, s7db.Num, 0, s7db.Len)
                            );
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