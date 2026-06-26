using UnityEngine;

public class TCPManager : MonoBehaviour
{
    public static TCPManager Instance;

    [Header("Configuração")]
    public bool host = true;

    public string ipServidor = "127.0.0.1";

    public int porta = 5000;

    private TCPServer servidor;
    private TCPClient cliente;

    private GameManager gameManager;

    public bool MeuTurno { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (host)
        {
            servidor = new TCPServer();
            servidor.StartServer(porta);

            // O Host sempre começa
            MeuTurno = true;
        }
        else
        {
            cliente = new TCPClient();

            if (cliente.Conectar(ipServidor, porta))
            {
                Debug.Log("Conectado ao servidor.");
            }
            else
            {
                Debug.LogError("Não foi possível conectar ao servidor.");
            }

            // Cliente espera o Host jogar
            MeuTurno = false;
        }
    }

    void Update()
    {
        string mensagem = null;

        if (host && servidor != null)
            mensagem = servidor.Receive();

        if (!host && cliente != null)
            mensagem = cliente.Receber();

        if (!string.IsNullOrEmpty(mensagem))
        {
            ProcessarMensagem(mensagem);
        }
    }

    void ProcessarMensagem(string msg)
    {
        if (msg.StartsWith("PLAY:"))
        {
            int coluna = int.Parse(msg.Substring(5));

            // Executa a jogada recebida
            gameManager.JogadaRecebida(coluna);

            // Agora é minha vez
            MeuTurno = true;
        }
    }

    public void EnviarJogada(int coluna)
    {
        string msg = "PLAY:" + coluna;

        if (host)
            servidor.Send(msg);
        else
            cliente.Enviar(msg);

        // Depois de jogar, espera o outro jogador
        MeuTurno = false;
    }

    void OnApplicationQuit()
    {
        if (cliente != null)
            cliente.Fechar();

        if (servidor != null)
            servidor.Fechar();
    }
}