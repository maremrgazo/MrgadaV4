#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0168 // Variable is declared but never used

using System.Net.Security;
using System.Net.Sockets;
using Serilog;

public static partial class Mrgada
{
    public class MrgadaTcpClient
    {
        private readonly string _serverName;
        private readonly string _serverIp;
        private readonly int _serverPort;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private Thread t_receiveBroadcast;
        private bool b_receiveBroadcast;
        private int i_receiveBroadcastTimeout;

        private Thread t_connectHandler;
        private bool b_connectHandler;
        private int i_connectHandlerTimeout;

        public bool Started;
        public bool Stopped;

        public bool Connected => _tcpClient?.Connected ?? false;
        public bool Disconnected => !Connected;

        public MrgadaTcpClient(string serverName, string serverIp, int serverPort, int connectHandlerTimeout = 3000, int receiveBroadcastTimeout = 200)
        {
            _serverName = serverName;
            _serverIp = serverIp;
            _serverPort = serverPort;
            Started = false;
            Stopped = true;

            i_connectHandlerTimeout = connectHandlerTimeout;
            i_receiveBroadcastTimeout = receiveBroadcastTimeout;
        }

        protected virtual void OnConnect() { }
        protected virtual void OnDisconnect() { }
        protected virtual void OnReceive(byte[] data) { }
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }

        public void Start()
        {
            b_connectHandler = true;
            t_connectHandler = new Thread(ConnectThread);
            t_connectHandler.IsBackground = true;
            t_connectHandler.Start();

            b_receiveBroadcast = true;
            t_receiveBroadcast = new Thread(ReceiveThread);
            t_receiveBroadcast.IsBackground = true;
            t_receiveBroadcast.Start();

            Started = true;
            Stopped = false;

            Log.Information($"Client Started {_serverName}");
            OnStart();
        }

        public void Stop()
        {
            b_receiveBroadcast = false;
            b_connectHandler = false;

            if (t_connectHandler.IsAlive) t_connectHandler.Join();
            if (t_receiveBroadcast.IsAlive) t_receiveBroadcast.Join();

            _networkStream.Close();
            _tcpClient.Close();

            Started = false;
            Stopped = true;

            Log.Information($"Client stopped: {_serverName}");
            OnStop();
        }

        private void ConnectThread()
        {
            while (b_connectHandler)
            {
                try
                {
                    if (!Connected)
                    {
                        _tcpClient = new TcpClient();
                        _tcpClient.Connect(_serverIp, _serverPort);
                        _networkStream = _tcpClient.GetStream();
                        Log.Information($"Connected to TCP Server: {_serverName}");
                        OnConnect();

                        while (Connected && b_connectHandler)
                        {
                            Thread.Sleep(i_connectHandlerTimeout);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Retrying connection in ~ {i_connectHandlerTimeout / 1000.0:F2} seconds: {_serverName}");
                    Thread.Sleep(i_connectHandlerTimeout);
                }
            }
        }

        public void Send(byte[] data)
        {
            if (!Connected || Stopped) return;
            try
            {
                _networkStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Log.Warning($"Tcp Client Error sending data to {_serverName}");
            }
        }

        private void ReceiveThread()
        {
            try
            {
                while (b_connectHandler)
                {
                    if (Connected)
                    {
                        if (_tcpClient.Available > 0)
                        {
                            byte[] buffer = new byte[_tcpClient.Available];
                            int bytesRead = _networkStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                OnReceive(buffer);
                            }
                        }
                        Thread.Sleep(100); // Reduce CPU usage
                    }
                    //Thread.Sleep(i_connectHandlerTimeout);
                    Thread.Sleep(i_receiveBroadcastTimeout); // če je prevelik sleep se buffer nafila in json zjebe
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Receive thread encountered an error: {ex.Message}");
                Stop();
            }
        }
    }
}
