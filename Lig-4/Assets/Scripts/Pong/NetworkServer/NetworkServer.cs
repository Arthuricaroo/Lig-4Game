using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkServer : MonoBehaviour
{
    public int port = 7777;

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Thread serverThread;

    public bool serverRunning = false;

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        serverThread = new Thread(ServerLoop);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void ServerLoop()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            serverRunning = true;

            Debug.Log("Servidor iniciado na porta " + port);

            client = server.AcceptTcpClient();

            Debug.Log("Cliente conectado!");

            stream = client.GetStream();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro no servidor: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            stream?.Close();
            client?.Close();
            server?.Stop();
            serverThread?.Abort();
        }
        catch { }
    }
}