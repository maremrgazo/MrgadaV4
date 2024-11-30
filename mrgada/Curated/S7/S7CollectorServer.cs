public static partial class Mrgada
{
    public class S7CollectorServer: MrgadaTcpServer
    {
        public S7CollectorServer(string serverName, string serverIp, int serverPort) : base(serverName, serverIp, serverPort)
        {
        }
    }
}