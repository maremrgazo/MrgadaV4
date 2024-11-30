public static partial class Mrgada
{
    public class S7Collector
    {
        private Mrgada.S7CollectorClient? _s7CollectorClient;
        private Mrgada.S7CollectorServer? _s7CollectorServer;

        public readonly string CollectorName;
        public readonly int CollectorPort;
        public readonly string PlcIp;

        public S7Collector(string collectorName, int collectorPort, string plcIp)
        {
            CollectorName = collectorName;
            CollectorPort = collectorPort;
            PlcIp = plcIp;

            Mrgada._s7Collectors.Add(this);
        }
        public void Start()
        {
            if (Mrgada.ServerIp == null) throw new Exception("ServerIp is not set in Init()!");

            switch (Mrgada.MachineType)
            {
                case e_MachineType.Server:
                    _s7CollectorServer = new(CollectorName, Mrgada.ServerIp, CollectorPort);
                    _s7CollectorServer.Start();
                    while (_s7CollectorServer.Stopped) Thread.Sleep(100); // Wait for server to start
                    


                    break;
                case e_MachineType.Client:
                    _s7CollectorClient = new(CollectorName, ServerIp, _serverPort);
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
                    _s7CollectorServer.Stop();
                    while (_s7CollectorServer.Started) Thread.Sleep(100); // Wait for client to start
                    break;
                case e_MachineType.Client:
                    _s7CollectorClient.Stop();
                    while (_s7CollectorClient.Started) Thread.Sleep(100); // Wait for client to start
                    break;
            }
        }
    }
}