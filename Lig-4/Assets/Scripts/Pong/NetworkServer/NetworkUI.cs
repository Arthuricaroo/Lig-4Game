using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    public NetworkManager networkManager;

    public void StartServer()
    {
        networkManager.StartServer();

        Debug.Log("Você é o SERVIDOR / PLAYER 1");
    }

    public void ConnectClient()
    {
        networkManager.ConnectToServer();

        Debug.Log("Você é o CLIENTE / PLAYER 2");
    }
}