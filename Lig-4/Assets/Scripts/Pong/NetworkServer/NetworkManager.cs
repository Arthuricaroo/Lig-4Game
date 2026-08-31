using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public enum NetworkMode
    {
        None,
        Server,
        Client
    }

    public NetworkMode mode = NetworkMode.None;

    public string serverIP = "127.0.0.1";
    public int port = 7777;

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;

    private Thread serverThread;
    private Thread receiveThread;

    public bool connected = false;

    // Dados recebidos do cliente
    public float clientPlayerY = 0f;

    // Dados recebidos do servidor
    public float player1Y;
    public float player2Y;
    public float ballX;
    public float ballY;
    public int score1;
    public int score2;

    void OnApplicationQuit()
    {
        StopNetwork();
    }

    // =========================
    // SERVIDOR
    // =========================

    public void StartServer()
    {
        mode = NetworkMode.Server;

        serverThread = new Thread(ServerLoop);
        serverThread.IsBackground = true;
        serverThread.Start();

        Debug.Log("Iniciando servidor...");
    }

    void ServerLoop()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            Debug.Log("Servidor iniciado na porta " + port);

            client = server.AcceptTcpClient();

            Debug.Log("Cliente conectado!");

            stream = client.GetStream();
            connected = true;

            receiveThread = new Thread(ReceiveFromClient);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro no servidor: " + e.Message);
        }
    }

    void ReceiveFromClient()
    {
        byte[] buffer = new byte[1024];

        while (connected)
        {
            try
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);

                if (bytes <= 0)
                    break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytes);

                if (message.StartsWith("P2:"))
                {
                    string value = message.Substring(3);

                    if (float.TryParse(
                        value,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out float y))
                    {
                        clientPlayerY = y;
                    }
                }
            }
            catch
            {
                break;
            }
        }

        connected = false;
    }

    public void SendGameState(
        float p1Y,
        float p2Y,
        float bX,
        float bY,
        int s1,
        int s2)
    {
        if (mode != NetworkMode.Server)
            return;

        if (!connected || stream == null)
            return;

        string message =
            "STATE|" +
            p1Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
            p2Y.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
            bX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
            bY.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
            s1 + "|" +
            s2;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch
        {
            connected = false;
        }
    }

    // =========================
    // CLIENTE
    // =========================

    public void ConnectToServer()
    {
        mode = NetworkMode.Client;

        receiveThread = new Thread(ClientConnect);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ClientConnect()
    {
        try
        {
            client = new TcpClient();

            Debug.Log("Conectando ao servidor " + serverIP);

            client.Connect(serverIP, port);

            stream = client.GetStream();
            connected = true;

            Debug.Log("Conectado ao servidor!");

            ReceiveGameState();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao conectar: " + e.Message);
        }
    }

    void ReceiveGameState()
    {
        byte[] buffer = new byte[2048];

        while (connected)
        {
            try
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);

                if (bytes <= 0)
                    break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytes);

                if (message.StartsWith("STATE|"))
                {
                    string[] data = message.Split('|');

                    if (data.Length >= 7)
                    {
                        float.TryParse(
                            data[1],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out player1Y);

                        float.TryParse(
                            data[2],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out player2Y);

                        float.TryParse(
                            data[3],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out ballX);

                        float.TryParse(
                            data[4],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out ballY);

                        int.TryParse(data[5], out score1);
                        int.TryParse(data[6], out score2);
                    }
                }
            }
            catch
            {
                break;
            }
        }

        connected = false;
    }

    public void SendPlayer2Position(float y)
    {
        if (mode != NetworkMode.Client)
            return;

        if (!connected || stream == null)
            return;

        string message =
            "P2:" +
            y.ToString(
                System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            stream.Write(data, 0, data.Length);
        }
        catch
        {
            connected = false;
        }
    }

    public void StopNetwork()
    {
        connected = false;

        try
        {
            stream?.Close();
            client?.Close();
            server?.Stop();

            if (serverThread != null && serverThread.IsAlive)
                serverThread.Abort();

            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Abort();
        }
        catch
        {
        }
    }
}