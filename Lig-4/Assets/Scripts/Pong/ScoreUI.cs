using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TMP_Text scoreText;

    private NetworkManager network;

    void Start()
    {
        network = FindObjectOfType<NetworkManager>();
    }

    void Update()
    {
        if (network == null)
            return;

        if (network.mode == NetworkManager.NetworkMode.Client)
        {
            scoreText.text =
                network.score1 + "   -   " + network.score2;
        }
    }
}