using System;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int port = 7777;

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient();

            client.Connect(serverIP, port);

            stream = client.GetStream();

            Debug.Log("Conectado ao servidor!");
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao conectar: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            stream?.Close();
            client?.Close();
        }
        catch { }
    }
}