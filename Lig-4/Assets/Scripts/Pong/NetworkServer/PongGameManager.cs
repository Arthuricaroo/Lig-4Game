using UnityEngine;

public class PongGameManager : MonoBehaviour
{
    public NetworkManager network;

    [Header("Objetos")]
    public Transform player1;
    public Transform player2;
    public Transform ball;

    [Header("Configuração")]
    public float playerSpeed = 6f;
    public float ballSpeed = 5f;

    [Header("Limites")]
    public float playerLimit = 3.2f;
    public float leftGoal = -8f;
    public float rightGoal = 8f;

    [Header("Placar")]
    public int score1 = 0;
    public int score2 = 0;

    private Vector2 ballDirection;

    private bool gameStarted = false;

    void Start()
    {
        if (network == null)
        {
            network =
                FindObjectOfType<NetworkManager>();
        }

        ResetBall(1);
    }

    void Update()
    {
        if (network == null)
            return;

        // =================================================
        // SERVIDOR
        // =================================================

        if (network.mode ==
            NetworkManager.NetworkMode.Server)
        {
            ServerUpdate();
        }

        // =================================================
        // CLIENTE
        // =================================================

        if (network.mode ==
            NetworkManager.NetworkMode.Client)
        {
            ClientUpdate();
        }
    }

    // =====================================================
    // SERVIDOR
    // =====================================================

    void ServerUpdate()
    {
        if (!network.connected)
            return;

        // Player 1 - teclado do servidor
        float input1 = 0f;

        if (Input.GetKey(KeyCode.W))
            input1 = 1f;

        if (Input.GetKey(KeyCode.S))
            input1 = -1f;

        MovePlayer(
            player1,
            input1);

        // Player 2 - input recebido pela rede
        float input2 =
            network.GetPlayer2Input();

        MovePlayer(
            player2,
            input2);

        // Bola
        if (network.isReadyToPlay)
        {
            gameStarted = true;
        }

        if (gameStarted)
        {
            MoveBall();
        }

        // Enviar estado
        network.SendGameState(
            player1.position.y,
            player2.position.y,
            ball.position.x,
            ball.position.y,
            score1,
            score2);
    }

    // =====================================================
    // CLIENTE
    // =====================================================

    void ClientUpdate()
    {
        // Player 2 envia apenas o comando
        float input = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            input = 1f;

        if (Input.GetKey(KeyCode.DownArrow))
            input = -1f;

        network.SendPlayer2Input(input);

        // Recebe posição do servidor
        Vector3 p1Position =
            player1.position;

        p1Position.y =
            Mathf.Lerp(
                p1Position.y,
                network.GetPlayer1Y(),
                15f * Time.deltaTime);

        player1.position =
            p1Position;

        Vector3 p2Position =
            player2.position;

        p2Position.y =
            Mathf.Lerp(
                p2Position.y,
                network.GetPlayer2Y(),
                15f * Time.deltaTime);

        player2.position =
            p2Position;

        Vector3 ballPosition =
            ball.position;

        ballPosition.x =
            Mathf.Lerp(
                ballPosition.x,
                network.GetBallX(),
                15f * Time.deltaTime);

        ballPosition.y =
            Mathf.Lerp(
                ballPosition.y,
                network.GetBallY(),
                15f * Time.deltaTime);

        ball.position =
            ballPosition;
    }

    // =====================================================
    // MOVIMENTO DO JOGADOR
    // =====================================================

    void MovePlayer(
        Transform player,
        float input)
    {
        Vector3 position =
            player.position;

        position.y +=
            input *
            playerSpeed *
            Time.deltaTime;

        position.y =
            Mathf.Clamp(
                position.y,
                -playerLimit,
                playerLimit);

        player.position =
            position;
    }

    // =====================================================
    // BOLA
    // =====================================================

    void MoveBall()
    {
        Vector3 position =
            ball.position;

        position.x +=
            ballDirection.x *
            ballSpeed *
            Time.deltaTime;

        position.y +=
            ballDirection.y *
            ballSpeed *
            Time.deltaTime;

        ball.position =
            position;

        // Parede superior
        if (ball.position.y >= 4.2f)
        {
            ballDirection.y =
                -Mathf.Abs(ballDirection.y);
        }

        // Parede inferior
        if (ball.position.y <= -4.2f)
        {
            ballDirection.y =
                Mathf.Abs(ballDirection.y);
        }

        // Player 1
        if (CheckPlayerCollision(player1))
        {
            ballDirection.x =
                Mathf.Abs(ballDirection.x);

            ballDirection.y =
                CalculateBounce(player1);
        }

        // Player 2
        if (CheckPlayerCollision(player2))
        {
            ballDirection.x =
                -Mathf.Abs(ballDirection.x);

            ballDirection.y =
                CalculateBounce(player2);
        }

        // Gol do Player 1
        if (ball.position.x > rightGoal)
        {
            score1++;

            ResetBall(-1);
        }

        // Gol do Player 2
        if (ball.position.x < leftGoal)
        {
            score2++;

            ResetBall(1);
        }
    }

    // =====================================================
    // COLISÃO COM RAQUETE
    // =====================================================

    bool CheckPlayerCollision(
        Transform player)
    {
        float distanceX =
            Mathf.Abs(
                ball.position.x -
                player.position.x);

        float distanceY =
            Mathf.Abs(
                ball.position.y -
                player.position.y);

        return
            distanceX < 0.45f &&
            distanceY < 1.3f;
    }

    // =====================================================
    // ÂNGULO DA BOLA
    // =====================================================

    float CalculateBounce(
        Transform player)
    {
        float difference =
            ball.position.y -
            player.position.y;

        return Mathf.Clamp(
            difference,
            -1f,
            1f);
    }

    // =====================================================
    // RESETAR BOLA
    // =====================================================

    void ResetBall(int direction)
    {
        ball.position =
            Vector3.zero;

        float randomY =
            Random.Range(
                -0.7f,
                0.7f);

        ballDirection =
            new Vector2(
                direction,
                randomY).normalized;
    }
}