using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 6f;
    public bool isPlayer1 = true;

    private NetworkManager network;

    void Start()
    {
        network = FindObjectOfType<NetworkManager>();
    }

    void Update()
    {
        if (network == null)
            return;

        // ===================================
        // SERVIDOR - PLAYER 1
        // ===================================

        if (network.mode == NetworkManager.NetworkMode.Server &&
            isPlayer1)
        {
            float input = 0f;

            if (Input.GetKey(KeyCode.W))
                input = 1f;

            if (Input.GetKey(KeyCode.S))
                input = -1f;

            Move(input);
        }

        // ===================================
        // SERVIDOR - RECEBE PLAYER 2
        // ===================================

        if (network.mode == NetworkManager.NetworkMode.Server &&
            !isPlayer1)
        {
            Vector3 position = transform.position;

            position.y = network.clientPlayerY;

            transform.position = position;
        }

        // ===================================
        // CLIENTE - PLAYER 2
        // ===================================

        if (network.mode == NetworkManager.NetworkMode.Client &&
            !isPlayer1)
        {
            float input = 0f;

            if (Input.GetKey(KeyCode.UpArrow))
                input = 1f;

            if (Input.GetKey(KeyCode.DownArrow))
                input = -1f;

            Move(input);

            network.SendPlayer2Position(
                transform.position.y);
        }

        // ===================================
        // CLIENTE RECEBE PLAYER 1
        // ===================================

        if (network.mode == NetworkManager.NetworkMode.Client &&
            isPlayer1)
        {
            Vector3 position = transform.position;

            position.y = Mathf.Lerp(
                position.y,
                network.player1Y,
                10f * Time.deltaTime);

            transform.position = position;
        }

        // ===================================
        // CLIENTE RECEBE PLAYER 2
        // ===================================

        if (network.mode == NetworkManager.NetworkMode.Client &&
            !isPlayer1)
        {
            Vector3 position = transform.position;

            position.y = Mathf.Lerp(
                position.y,
                network.player2Y,
                10f * Time.deltaTime);

            transform.position = position;
        }
    }

    void Move(float input)
    {
        Vector3 position = transform.position;

        position.y += input * speed * Time.deltaTime;

        position.y = Mathf.Clamp(
            position.y,
            -3.2f,
            3.2f);

        transform.position = position;
    }
}