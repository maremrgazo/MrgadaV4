#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0168 // Variable is declared but never used

using System.Net.Sockets;
using System.Threading;
using Serilog;

public static partial class Mrgada
{
    public class MrgadaTcpClient
    {
        // Initalization
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly string _ServerName;
        private readonly int _tryToConnectTimeout;
        private readonly int _broadcastListenTimeout;

        // Thread to try to connect to the server
        private Thread _tryToConnect;

        // Thread to listen to server broadcasts
        private Thread _broadcastListen;

        // Tcp Client
        private byte[] _broadcastBuffer;
        private readonly int _broadcastBufferLength;
        private NetworkStream _stream;
        private TcpClient _client;
        public bool IsConnected;
        private CancellationTokenSource _cancellationTokenSource;

        public MrgadaTcpClient(String ServerName, String ServerIp, int ServerPort, int BroadcastBufferLength = 65563, int TryToConnectTimeout = 3000, int BroadcastListenTimeout = 200) 
        {
            _serverIp = ServerIp;
            _serverPort = ServerPort;
            _ServerName = ServerName;
            _tryToConnectTimeout = TryToConnectTimeout;
            _broadcastListenTimeout = BroadcastListenTimeout;

            _broadcastBufferLength = BroadcastBufferLength;
            _broadcastBuffer = new byte[_broadcastBufferLength];

            IsConnected = false;
        }

        public virtual void OnBroadcastReceived(byte[] BroadcastBuffer) 
        {
            Log.Information($"Received broadcast from TCP Server: {_ServerName}");
        }

        public void Start()
        {
            _tryToConnect = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!IsConnected)
                        {
                            _client = new TcpClient();
                            _client.Connect(_serverIp, _serverPort);
                            Log.Information($"Connected to TCP Server: {_ServerName}");

                            _stream = _client.GetStream();
                            IsConnected = true;

                            // Initialize a new cancellation token source for each connection
                            _cancellationTokenSource = new CancellationTokenSource();

                            // Start the ClientBroadcastListenThread when connected
                            StartBroadcastListenThread(_cancellationTokenSource.Token);

                            while (IsConnected)
                            {
                                Thread.Sleep(_tryToConnectTimeout);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IsConnected = false;
                        Log.Information($"Retrying connection in ~ {_tryToConnectTimeout / 1000.0:F2} seconds: {_ServerName}");
                        Thread.Sleep(_tryToConnectTimeout);
                    }
                }
            
            }));

            _tryToConnect.IsBackground = true;
            _tryToConnect.Start();
        }
        private void StartBroadcastListenThread(CancellationToken _cancellationToken)
        {
            _broadcastListen = new Thread(() =>
            {
                try
                {
                    while (IsConnected && !_cancellationToken.IsCancellationRequested)
                    {
                        if (_stream.CanRead)
                        {
                            int bytesRead = _stream.Read(_broadcastBuffer, 0, _broadcastBufferLength);
                            if (bytesRead == 0)
                            {
                                Log.Information($"TCP Server has closed the connection: {_ServerName}");
                                IsConnected = false;
                                break;
                            }

                            OnBroadcastReceived(_broadcastBuffer);
                            _broadcastBuffer = new byte[_broadcastBufferLength]; // Clear the buffer

                        }
                        Thread.Sleep(_broadcastListenTimeout);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Connection lost to TCP Server while listening for broadcast: {_ServerName}");
                    IsConnected = false;
                }
                finally
                {
                    // Clean up and reset connection state
                    Disconnect();
                }
            });
            _broadcastListen.IsBackground = true;
            _broadcastListen.Start();
        }

        private void Disconnect()
        {

            if (_client != null)
            {
                try
                {
                    if (_stream != null)
                    {
                        _stream.Close();
                        _stream.Dispose();
                    }

                    _client.Close();
                    _client.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error during disconnect from TCP Server: {_ServerName}");
                }
                finally
                {
                    Log.Information($"Disconnected from TCP Server: {_ServerName}");
                    IsConnected = false;

                    // Cancel the listening thread gracefully
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                    }

                    if (_broadcastListen != null && _broadcastListen.IsAlive)
                    {
                        _broadcastListen.Join(); // Wait for the thread to exit
                    }
                }
            }
        }
    }
}