using Serilog;

public static partial class Mrgada
{
    public class S7CollectorClient: MrgadaTcpClient
    {
        private string _collectorName;
        public S7CollectorClient(string collectorName, string serverIp, int serverPort, int connectHandlerTimeout = 3000, int receiveBroadcastTimeout = 200) : base(collectorName, serverIp, serverPort, connectHandlerTimeout, receiveBroadcastTimeout)
        {
            _collectorName = collectorName;
        }

        protected override void OnReceive(byte[] data)
        {
            Int32 broadcastLength = BitConverter.ToInt32(data, 0);
            bool isPartial = data.Length != broadcastLength;
            Log.Information($"Client Recieved Broadcast, isPartial ({isPartial}) len ({data.Length}) from S7 Collector: {_collectorName}");
        }
    }
}