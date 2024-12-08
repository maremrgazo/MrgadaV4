using System;
using System.Collections.Generic;
using System.Threading;

public static partial class Mrgada
{
    public enum e_MachineType
    {
        Server,
        Client
    } 
    public static string? ServerIp { get; private set; }
    private static int _serverPort;
    public static e_MachineType MachineType;

    private static Mrgada.SyncVarClient? _mrgadaSyncVarClient;
    private static Mrgada.SyncVarServer? _mrgadaSyncVarServer;
    private static Thread? _mrgadaSyncVarServerTask;  
    
    private static List<S7Collector> _s7Collectors = [];
    // opc, rockwell, etc

    public static void Init(string serverIp, int serverPort, e_MachineType machineType)
    {
        ServerIp = serverIp;
        _serverPort = serverPort;
        MachineType = machineType;

        InitCollectors();
    }

    public static void Start()
    {
        if (ServerIp == null) throw new Exception("ServerIp is not set in Init()!");

        switch (MachineType)
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
        StartCollectors();
    }
    public static void StartCollectors()
    {
        foreach (var collector in _s7Collectors)
        {
            collector.Start();
        }
        // opc, rockwell, etc
    }
    public static void StopCollectors()
    {
        foreach (var collector in _s7Collectors)
        {
            collector.Stop();
        }
        // opc, rockwell, etc
    }
    public static void Stop() 
    {
        switch (MachineType)
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
        StopCollectors();
    }
}