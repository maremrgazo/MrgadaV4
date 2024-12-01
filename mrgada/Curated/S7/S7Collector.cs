using S7.Net;
using static Mrgada.S7Collector;

public static partial class Mrgada
{
    public partial class S7Collector
    {
        private Mrgada.S7CollectorClient? _s7CollectorClient;
        private Mrgada.S7CollectorServer? _s7CollectorServer;
        private List<S7db> _s7dbs = [];

        public readonly string CollectorName;
        public readonly int CollectorPort;

        private readonly string _plcIp;
        private S7.Net.CpuType _cpuType;
        private short _plcRack;
        private short _plcSlot;

        protected void AddS7db(S7db s7db)
        {
            _s7dbs.Add(s7db);
        }

        public S7Collector(string collectorName, int collectorPort, string plcIp, S7.Net.CpuType cpuType, short plcRack, short plcSlot)
        {
            CollectorName = collectorName;
            CollectorPort = collectorPort;
            _plcIp = plcIp;
            _cpuType = cpuType;
            _plcRack = plcRack;
            _plcSlot = plcSlot;

            Mrgada._s7Collectors.Add(this);
        }
        public void Start()
        {
            if (Mrgada.ServerIp == null) throw new Exception("ServerIp is not set in Init()!");

            switch (Mrgada.MachineType)
            {
                case e_MachineType.Server:
                    _s7CollectorServer = new(_s7dbs, CollectorName, Mrgada.ServerIp, CollectorPort, _cpuType, _plcIp, _plcRack, _plcSlot);
                    _s7CollectorServer.Start();
                    while (_s7CollectorServer.Stopped) Thread.Sleep(100); // Wait for server to start
                    


                    break;
                case e_MachineType.Client:
                    _s7CollectorClient = new(_s7dbs, CollectorName, Mrgada.ServerIp, CollectorPort);
                    _s7CollectorClient.Start();
                    while (_s7CollectorClient.Stopped) Thread.Sleep(100); // Wait for client to start



                    break;
            }
        }
        public void Stop()
        {
            switch (Mrgada.MachineType)
            {
                case e_MachineType.Server:
                    if (_s7CollectorServer == null) throw new Exception("S7CollectorServer was never Started!");
                    _s7CollectorServer.Stop();
                    while (_s7CollectorServer.Started) Thread.Sleep(100); // Wait for server to stop
                    break;
                case e_MachineType.Client:
                    if (_s7CollectorClient == null) throw new Exception("S7CollectorClient was never Started!");
                    _s7CollectorClient.Stop();
                    while (_s7CollectorClient.Started) Thread.Sleep(100); // Wait for client to stop
                    break;
            }
        }
    }
}