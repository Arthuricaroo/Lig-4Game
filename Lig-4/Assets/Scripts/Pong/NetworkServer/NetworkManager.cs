using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public enum NetworkMode
    {
        None,
        Server,
        Client
    }

    [Header("Rede")]
    public NetworkMode mode = NetworkMode.None;

    public string serverIP = "127.0.0.1";
    public int port = 7777;

    [Header("Estado da conexão")]
    public bool connected = false;
    public bool isReadyToPlay = false;

    // =========================================================
    // SERVIDOR
    // =========================================================

    private TcpListener server;
    private TcpClient serverClient;
    private NetworkStream serverStream;

    private Thread serverThread;
    private Thread serverReceiveThread;

    // =========================================================
    // CLIENTE
    // =========================================================

    private TcpClient client;
    private NetworkStream clientStream;

    private Thread clientThread;
    private Thread clientReceiveThread;

    // =========================================================
    // DADOS RECEBIDOS
    // =========================================================

    public float player2Input = 0f;

    public float player1Y = 0f;
    public float player2Y = 0f;

    public float ballX = 0f;
    public float ballY = 0f;

    public int score1 = 0;
    public int score2 = 0;

    private readonly object dataLock = new object();

    private readonly ConcurrentQueue<string> receivedMessages =
        new ConcurrentQueue<string>();

    private bool shouldLoadGame = false;

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // A troca de cena precisa acontecer na thread principal
        if (shouldLoadGame)
        {
            shouldLoadGame = false;

            if (SceneManager.GetActiveScene().name != "Pong")
            {
                SceneManager.LoadScene("Pong");
            }
        }

        ProcessReceivedMessages();
    }

    // =========================================================
    // INICIAR SERVIDOR
    // =========================================================

    public void StartServer()
    {
        if (mode != NetworkMode.None)
            return;

        mode = NetworkMode.Server;

        Debug.Log("Iniciando servidor...");

        serverThread = new Thread(ServerThread);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void ServerThread()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);

            server.Start();

            Debug.Log("Servidor iniciado na porta " + port);

            Debug.Log("Aguardando Jogador 2...");

            serverClient = server.AcceptTcpClient();

            serverStream = serverClient.GetStream();

            connected = true;

            Debug.Log("Jogador 2 conectado!");

            shouldLoadGame = true;

            serverReceiveThread =
                new Thread(ServerReceiveThread);

            serverReceiveThread.IsBackground = true;
            serverReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro no servidor: " + e.Message);
        }
    }

    // =========================================================
    // RECEBER DADOS DO CLIENTE
    // =========================================================

    private void ServerReceiveThread()
    {
        try
        {
            using (StreamReader reader =
                   new StreamReader(serverStream, Encoding.UTF8))
            {
                while (connected)
                {
                    string message = reader.ReadLine();

                    if (message == null)
                        break;

                    receivedMessages.Enqueue(message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Erro recebendo dados do cliente: " + e.Message);
        }

        connected = false;
    }

    // =========================================================
    // PROCESSAR MENSAGENS RECEBIDAS
    // =========================================================

    private void ProcessReceivedMessages()
    {
        while (receivedMessages.TryDequeue(out string message))
        {
            if (message.StartsWith("INPUT|"))
            {
                string[] parts = message.Split('|');

                if (parts.Length >= 2)
                {
                    if (float.TryParse(
                        parts[1],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float input))
                    {
                        lock (dataLock)
                        {
                            player2Input = Mathf.Clamp(input, -1f, 1f);
                        }
                    }
                }
            }

            if (message == "READY")
            {
                isReadyToPlay = true;
            }
        }
    }

    // =========================================================
    // ENVIAR ESTADO DO JOGO
    // =========================================================

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

        if (!connected || serverStream == null)
            return;

        string message =
            "STATE|" +
            p1Y.ToString(CultureInfo.InvariantCulture) + "|" +
            p2Y.ToString(CultureInfo.InvariantCulture) + "|" +
            bX.ToString(CultureInfo.InvariantCulture) + "|" +
            bY.ToString(CultureInfo.InvariantCulture) + "|" +
            s1 + "|" +
            s2;

        SendLine(serverStream, message);
    }

    // =========================================================
    // CONECTAR COMO CLIENTE
    // =========================================================

    public void ConnectToServer()
    {
        if (mode != NetworkMode.None)
            return;

        mode = NetworkMode.Client;

        Debug.Log("Conectando ao servidor " + serverIP);

        clientThread = new Thread(ClientConnectThread);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientConnectThread()
    {
        try
        {
            client = new TcpClient();

            client.Connect(serverIP, port);

            clientStream = client.GetStream();

            connected = true;

            Debug.Log("Conectado ao servidor!");

            // Informa que o cliente está pronto
            SendLine(clientStream, "READY");

            shouldLoadGame = true;

            clientReceiveThread =
                new Thread(ClientReceiveThread);

            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao conectar: " + e.Message);
        }
    }

    // =========================================================
    // RECEBER ESTADO DO SERVIDOR
    // =========================================================

    private void ClientReceiveThread()
    {
        try
        {
            using (StreamReader reader =
                   new StreamReader(clientStream, Encoding.UTF8))
            {
                while (connected)
                {
                    string message = reader.ReadLine();

                    if (message == null)
                        break;

                    receivedMessages.Enqueue(message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Erro recebendo estado: " + e.Message);
        }

        connected = false;
    }

    // =========================================================
    // PROCESSAMENTO DO ESTADO DO SERVIDOR
    // =========================================================

    private void ProcessStateMessage(string message)
    {
        string[] parts = message.Split('|');

        if (parts.Length < 7)
            return;

        float.TryParse(
            parts[1],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float p1);

        float.TryParse(
            parts[2],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float p2);

        float.TryParse(
            parts[3],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float bx);

        float.TryParse(
            parts[4],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float by);

        int.TryParse(parts[5], out int s1);
        int.TryParse(parts[6], out int s2);

        lock (dataLock)
        {
            player1Y = p1;
            player2Y = p2;

            ballX = bx;
            ballY = by;

            score1 = s1;
            score2 = s2;
        }
    }

    // =========================================================
    // ENVIAR INPUT DO PLAYER 2
    // =========================================================

    public void SendPlayer2Input(float input)
    {
        if (mode != NetworkMode.Client)
            return;

        if (!connected || clientStream == null)
            return;

        string message =
            "INPUT|" +
            input.ToString(CultureInfo.InvariantCulture);

        SendLine(clientStream, message);
    }

    // =========================================================
    // ENVIO TCP
    // =========================================================

    private void SendLine(NetworkStream stream, string message)
    {
        try
        {
            byte[] data =
                Encoding.UTF8.GetBytes(message + "\n");

            stream.Write(data, 0, data.Length);
        }
        catch
        {
            connected = false;
        }
    }

    // =========================================================
    // PROCESSAMENTO DE MENSAGENS
    // =========================================================

    private void ProcessMessageType(string message)
    {
        if (message.StartsWith("STATE|"))
        {
            ProcessStateMessage(message);
        }
    }

    // =========================================================
    // ATUALIZAÇÃO DO PROCESSAMENTO
    // =========================================================

    void LateUpdate()
    {
        while (receivedMessages.TryDequeue(out string message))
        {
            if (message.StartsWith("STATE|"))
            {
                ProcessStateMessage(message);
            }
            else if (message.StartsWith("INPUT|"))
            {
                string[] parts = message.Split('|');

                if (parts.Length >= 2 &&
                    float.TryParse(
                        parts[1],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float input))
                {
                    lock (dataLock)
                    {
                        player2Input =
                            Mathf.Clamp(input, -1f, 1f);
                    }
                }
            }
        }
    }

    // =========================================================
    // PARAR REDE
    // =========================================================

    public void StopNetwork()
    {
        connected = false;

        try
        {
            serverStream?.Close();
            serverClient?.Close();
            server?.Stop();

            clientStream?.Close();
            client?.Close();
        }
        catch
        {
        }
    }

    void OnApplicationQuit()
    {
        StopNetwork();
    }

    // =========================================================
    // MÉTODOS PÚBLICOS
    // =========================================================

    public float GetPlayer2Input()
    {
        lock (dataLock)
        {
            return player2Input;
        }
    }

    public float GetPlayer1Y()
    {
        lock (dataLock)
        {
            return player1Y;
        }
    }

    public float GetPlayer2Y()
    {
        lock (dataLock)
        {
            return player2Y;
        }
    }

    public float GetBallX()
    {
        lock (dataLock)
        {
            return ballX;
        }
    }

    public float GetBallY()
    {
        lock (dataLock)
        {
            return ballY;
        }
    }

    public int GetScore1()
    {
        lock (dataLock)
        {
            return score1;
        }
    }

    public int GetScore2()
    {
        lock (dataLock)
        {
            return score2;
        }
    }
}