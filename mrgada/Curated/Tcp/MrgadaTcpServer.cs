#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0168 // Variable is declared but never used

using System.Net;
using System.Net.Sockets;
using Serilog;

public static partial class Mrgada
{
    public class MrgadaTcpServer
    {
        private readonly string _serverName;
        private readonly string _serverIp;
        private readonly int _serverPort;

        public bool Started;
        public bool Stopped;

        private TcpListener tcpl_listener;

        List<TcpClient> l_clients = [];

        Thread t_clientConnect;
        bool b_clientConnect;

        Thread t_clientReceive;
        bool b_clientReceive;

        Thread t_clientMonitor;
        bool b_clientMonitor;
        int i_clientMonitorTimeout = 1000;

        private object o_clientsLock = new object();

        public MrgadaTcpServer(string serverName, string serverIp, int serverPort)
        {
            _serverName = serverName;
            _serverIp = serverIp;
            _serverPort = serverPort;

            Started = false;
            Stopped = true;
        }

        protected virtual void OnConnect(TcpClient Client) { }
        protected virtual void OnDisconnect(TcpClient Client) { }
        protected virtual void OnReceive(TcpClient Client, byte[] Buffer) { }
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }

        public void Start()
        {
            tcpl_listener = new TcpListener(IPAddress.Parse(_serverIp), _serverPort);
            tcpl_listener.Start();

            b_clientConnect = true;
            t_clientConnect = new Thread(ClientConnectThread);
            t_clientConnect.IsBackground = true;
            t_clientConnect.Start();

            b_clientMonitor = true;
            t_clientMonitor = new Thread(ClientMonitorThread);
            t_clientMonitor.IsBackground = true;
            t_clientMonitor.Start();

            b_clientReceive = true;
            t_clientReceive = new Thread(ClientReceiveThread);
            t_clientReceive.IsBackground = true;
            t_clientReceive.Start();

            Started = true;
            Stopped = false;

            Log.Information($"Tcp Server Started: {_serverName}");

            OnStart();
        }
        public void Stop()
        {
            b_clientConnect = false;
            b_clientMonitor = false;

            t_clientConnect.Join();
            t_clientMonitor.Join();

            Started = false;
            Stopped = true;

            Log.Information($"Tcp Server Stopped: {_serverName}");

            OnStop();
        }
        public void Broadcast(byte[] data)
        {
            if (l_clients.Count == 0 || Stopped) return;
            lock (o_clientsLock)
            {
                foreach (TcpClient client in l_clients)
                {
                    try
                    {
                        NetworkStream ns_stream = client.GetStream();
                        ns_stream.Write(data, 0, data.Length);
                    }
                    catch
                    {
                        // client disconnected while broadcasting
                        Log.Information($"Client disconnected while broadcasting: {_serverName}");
                    }
                }
            }
        }
        private void ClientConnectThread()
        {
            while (b_clientConnect)
            {
                try
                {
                    DateTime startTime = DateTime.Now;
                    TcpClient client = null;
                    while ((DateTime.Now - startTime).TotalSeconds < 3)
                    {
                        if (tcpl_listener.Pending())
                        {
                            client = tcpl_listener.AcceptTcpClient();
                            break;
                        }
                        Thread.Sleep(200); // Sleep for a short interval to avoid busy waiting
                    }
                    if (client != null) 
                    {
                        string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        lock (o_clientsLock)
                        {
                            l_clients.Add(client);
                            Log.Information($"Client({l_clients.Count}) {clientIp} Connected to TCP Server {_serverName}");
                        }
                        OnConnect(client);
                    }
                }

                catch (SocketException)
                {
                    // This exception is expected when the server stops, so we just break the loop.
                    Log.Information("This exception is expected when the server stops, so we just break the loop.");
                    break;
                }
            }
        }
        
        private void ClientMonitorThread()
        {
            while (b_clientMonitor)
            {
                lock (o_clientsLock)
                {
                    if (l_clients.Count != 0)
                    {
                        for (int i = l_clients.Count - 1; i >= 0; i--)
                        {
                            TcpClient client = l_clients[i];

                            if (!IsClientConnected(client))
                            {
                                string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                                Log.Information($"Client({l_clients.Count}) {clientIp} disconnected from TCP Server {_serverName}");
                                // Remove the client from the list
                                l_clients.RemoveAt(i);
                                // Optionally, close the client to free resources
                                client.Close();
                            }
                        }
                    }
                }
                // Sleep for a short duration to reduce CPU usage
                Thread.Sleep(i_clientMonitorTimeout);
            }
        }

        private void ClientReceiveThread()
        {
            while (b_clientReceive)
            {
                lock (o_clientsLock)
                {
                    foreach (var client in l_clients)
                    {
                        try
                        {
                            if (client.Available > 0) // Check if data is available
                            {
                                NetworkStream ns_stream = client.GetStream();
                                byte[] buffer = new byte[client.Available];
                                int bytesRead = ns_stream.Read(buffer, 0, buffer.Length);

                                if (bytesRead > 0)
                                {
                                    OnReceive(client, buffer);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning($"Error receiving data from client: {ex.Message}");
                            // Handle exceptions for individual clients, such as disconnections
                        }
                    }
                }
                Thread.Sleep(50); // Short sleep to reduce CPU usage
            }
        }

        private bool IsClientConnected(TcpClient client)
        {
            try
            {
                // Check if the client is connected by checking the state of the socket
                return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
            }
            catch (SocketException)
            {
                // If there's a socket exception, the client is definitely not connected
                Log.Information("If there's a socket exception, the client is definitely not connected");
                return false;
            }
        }
    }
}