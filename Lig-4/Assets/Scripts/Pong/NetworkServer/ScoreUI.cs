using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TMP_Text scoreText;

    public NetworkManager network;

    void Start()
    {
        if (network == null)
        {
            network =
                FindObjectOfType<NetworkManager>();
        }
    }

    void Update()
    {
        if (network == null)
            return;

        if (network.mode ==
            NetworkManager.NetworkMode.Server)
        {
            PongGameManager game =
                FindObjectOfType<PongGameManager>();

            if (game != null)
            {
                scoreText.text =
                    game.score1 +
                    "     -     " +
                    game.score2;
            }
        }

        if (network.mode ==
            NetworkManager.NetworkMode.Client)
        {
            scoreText.text =
                network.GetScore1() +
                "     -     " +
                network.GetScore2();
        }
    }
}