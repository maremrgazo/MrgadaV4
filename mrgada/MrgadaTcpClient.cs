#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0168 // Variable is declared but never used

using System.Net.Sockets;
using Serilog;

public static partial class Mrgada
{
    public class MrgadaTcpClient
    {
        // Initalization
        private readonly string s_serverIp;
        private readonly string s_serverName;
        private readonly int _serverPort;
        private readonly int _tryToConnectTimeout;
        private readonly int _listenForBroadcastTimeout;

        // Thread to try to connect to the server
        private Thread t_tryToConnect;

        // Thread to listen to server broadcasts
        private Thread t_listenForBroadcast;
        private CancellationTokenSource cts_listenForBroadcast;

        // Tcp Client
        public bool IsConnected;
        private byte[] _broadcastBuffer;
        private readonly int _broadcastBufferLength;
        private NetworkStream ns_stream;
        private TcpClient tcpc_client;

        public MrgadaTcpClient(String serverName, String serverIp, int serverPort, int broadcastBufferLength = 65536, int tryToConnectTimeout = 3000, int broadcastListenTimeout = 200) 
        {
            s_serverIp = serverIp;
            s_serverName = serverName;
            _serverPort = serverPort;
            _tryToConnectTimeout = tryToConnectTimeout;
            _listenForBroadcastTimeout = broadcastListenTimeout;

            _broadcastBufferLength = broadcastBufferLength;
            _broadcastBuffer = new byte[_broadcastBufferLength];

            IsConnected = false;
        }

        public virtual void OnBroadcastReceived(byte[] broadcastBuffer) 
        {
            Log.Information($"Received broadcast from TCP Server: {s_serverName}");
        }

        public void Start()
        {
            t_tryToConnect = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!IsConnected)
                        {
                            tcpc_client = new TcpClient();
                            tcpc_client.Connect(s_serverIp, _serverPort);
                            Log.Information($"Connected to TCP Server: {s_serverName}");

                            ns_stream = tcpc_client.GetStream();
                            IsConnected = true;

                            // Initialize a new cancellation token source for each connection
                            cts_listenForBroadcast = new CancellationTokenSource();

                            // Start the ClientBroadcastListenThread when connected
                            StartBroadcastListenThread(cts_listenForBroadcast.Token);

                            while (IsConnected)
                            {
                                Thread.Sleep(_tryToConnectTimeout);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IsConnected = false;
                        Log.Information($"Retrying connection in ~ {_tryToConnectTimeout / 1000.0:F2} seconds: {s_serverName}");
                        Thread.Sleep(_tryToConnectTimeout);
                    }
                }
            
            }));

            t_tryToConnect.IsBackground = true;
            t_tryToConnect.Start();
        }
        private void StartBroadcastListenThread(CancellationToken _cancellationToken)
        {
            t_listenForBroadcast = new Thread(() =>
            {
                try
                {
                    while (IsConnected && !_cancellationToken.IsCancellationRequested)
                    {
                        if (ns_stream.CanRead)
                        {
                            int bytesRead = ns_stream.Read(_broadcastBuffer, 0, _broadcastBufferLength);
                            if (bytesRead == 0)
                            {
                                Log.Information($"TCP Server has closed the connection: {s_serverName}");
                                IsConnected = false;
                                break;
                            }

                            OnBroadcastReceived(_broadcastBuffer);
                            _broadcastBuffer = new byte[_broadcastBufferLength]; // Clear the buffer

                        }
                        Thread.Sleep(_listenForBroadcastTimeout);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Connection lost to TCP Server while listening for broadcast: {s_serverName}");
                    IsConnected = false;
                }
                finally
                {
                    // Clean up and reset connection state
                    Disconnect();
                }
            });
            t_listenForBroadcast.IsBackground = true;
            t_listenForBroadcast.Start();
        }

        private void Disconnect()
        {

            if (tcpc_client != null)
            {
                try
                {
                    if (ns_stream != null)
                    {
                        ns_stream.Close();
                        ns_stream.Dispose();
                    }

                    tcpc_client.Close();
                    tcpc_client.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error during disconnect from TCP Server: {s_serverName}");
                }
                finally
                {
                    Log.Information($"Disconnected from TCP Server: {s_serverName}");
                    IsConnected = false;

                    // Cancel the listening thread gracefully
                    if (cts_listenForBroadcast != null)
                    {
                        cts_listenForBroadcast.Cancel();
                        cts_listenForBroadcast.Dispose();
                    }

                    if (t_listenForBroadcast != null && t_listenForBroadcast.IsAlive)
                    {
                        t_listenForBroadcast.Join(); // Wait for the thread to exit
                    }
                }
            }
        }
    }
}