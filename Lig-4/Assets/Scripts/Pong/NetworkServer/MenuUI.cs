using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [Header("Rede")]
    public NetworkManager networkManager;

    [Header("Interface")]
    public TMP_InputField ipInput;
    public TMP_Text statusText;

    [Header("Botões")]
    public Button serverButton;
    public Button clientButton;

    void Start()
    {
        statusText.text = "Escolha como deseja jogar.";

        serverButton.interactable = true;
        clientButton.interactable = true;
    }

    public void CreateServer()
    {
        serverButton.interactable = false;
        clientButton.interactable = false;

        statusText.text =
            "SERVIDOR CRIADO!\n" +
            "Aguardando Jogador 2...";

        networkManager.StartServer();
    }

    public void ConnectToServer()
    {
        string ip = ipInput.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            statusText.text =
                "Digite o IP do servidor.";

            return;
        }

        serverButton.interactable = false;
        clientButton.interactable = false;

        statusText.text =
            "Conectando ao servidor...";

        networkManager.serverIP = ip;

        networkManager.ConnectToServer();
    }

    void Update()
    {
        if (networkManager == null)
            return;

        if (networkManager.mode ==
            NetworkManager.NetworkMode.Server)
        {
            if (networkManager.connected)
            {
                statusText.text =
                    "Jogador 2 conectado!\n" +
                    "Iniciando partida...";
            }
        }

        if (networkManager.mode ==
            NetworkManager.NetworkMode.Client)
        {
            if (networkManager.connected)
            {
                statusText.text =
                    "Conectado!\n" +
                    "Você é o Jogador 2.";
            }
        }
    }
}