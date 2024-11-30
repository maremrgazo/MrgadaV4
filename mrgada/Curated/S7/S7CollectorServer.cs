using S7.Net;
using Serilog;

public static partial class Mrgada
{
    public class S7CollectorServer: MrgadaTcpServer
    {
        

        private S7.Net.Plc? _s7Plc;
        private S7.Net.CpuType _cpuType;
        private string _plcIp;
        private short _plcRack;
        private short _plcSlot;

        private Thread? t_collector;
        private bool b_collector;
        private string _collectorName;

        public bool CollectorConnected => _s7Plc?.IsConnected ?? false;
        public bool CollectorDisconnected => !CollectorConnected;

        public S7CollectorServer(string collectorName, string serverIp, int serverPort, S7.Net.CpuType cpuType, string plcIp, short plcRack, short plcSlot) : base(collectorName, serverIp, serverPort)
        {
            _cpuType = cpuType;
            _plcIp = plcIp;
            _collectorName = collectorName;
        }

        protected override void OnStart()
        {
            _s7Plc = new(_cpuType, _plcIp, _plcRack, _plcSlot);

            t_collector = new(CollectorThread);
            t_collector.IsBackground = true;
            t_collector.Start();
        }

        public void CollectorThread() 
        {
            while (b_collector)
            {
                if (CollectorConnected)
                {

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
                        Log.Information($"+Can't connect to S7 PLC, trying again in 30 seconds: {_collectorName}");
                        Thread.Sleep(30000);
                    }
                }
            }
        }
    }
}