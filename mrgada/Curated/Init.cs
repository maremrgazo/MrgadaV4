public static partial class Mrgada
{
    public enum e_MachineType
    {
        Server,
        Client
    } 
    public static string? ServerIp { get; private set; }
    private static int _serverPort;
    private static e_MachineType _machineType;

    private static Mrgada.SyncVarClient? _mrgadaSyncVarClient;
    private static Mrgada.SyncVarServer? _mrgadaSyncVarServer;
    private static Thread? _mrgadaSyncVarServerTask;   

    public static void Init(string serverIp, int serverPort, e_MachineType machineType)
    {
        ServerIp = serverIp;
        _serverPort = serverPort;
        _machineType = machineType;
    }

    public static void Start()
    {
        if (ServerIp == null) throw new Exception("ServerIp is not set in Init()!");

        switch (_machineType)
        {
            case e_MachineType.Server:
                _mrgadaSyncVarServer = new ("Mrgada", ServerIp, _serverPort);
                _mrgadaSyncVarServer.Start();
                while(_mrgadaSyncVarServer.Stopped) Thread.Sleep(100); // Wait for server to start
                _mrgadaSyncVarServerTask = new Thread(_mrgadaSyncVarServer.SyncVarServerThread);
                _mrgadaSyncVarServerTask.IsBackground = true;
                _mrgadaSyncVarServerTask.Start();
                break;

            case e_MachineType.Client:
                _mrgadaSyncVarClient = new ("Mrgada", ServerIp, _serverPort);
                _mrgadaSyncVarClient.Start();
                while (_mrgadaSyncVarClient.Stopped) Thread.Sleep(100); // Wait for client to start
                break;
        }
    }
    public static void Stop() 
    {
        switch (_machineType)
        {
            case e_MachineType.Server:
                if (_mrgadaSyncVarServer == null) throw new Exception("Start() was never called!");
                _mrgadaSyncVarServer.Stop();
                while (_mrgadaSyncVarServer.Started) Thread.Sleep(100);

                break;
            case e_MachineType.Client:
                if (_mrgadaSyncVarClient == null) throw new Exception("Start() was never called!");
                _mrgadaSyncVarClient.Stop();
                while (_mrgadaSyncVarClient.Started) Thread.Sleep(100);
                break;
        }
    }
}