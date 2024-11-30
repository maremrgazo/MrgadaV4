public static partial class Mrgada
{
    public class S7CollectorClient: MrgadaTcpClient
    {
        public S7CollectorClient(string serverName, string serverIp, int serverPort, int connectHandlerTimeout = 3000, int receiveBroadcastTimeout = 200) : base(serverName, serverIp, serverPort, connectHandlerTimeout, receiveBroadcastTimeout)
        {
        }
    }
}