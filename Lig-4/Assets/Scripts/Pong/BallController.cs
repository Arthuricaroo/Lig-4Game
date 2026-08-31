using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 5f;

    private Vector2 direction;

    private NetworkManager network;

    private int score1 = 0;
    private int score2 = 0;

    void Start()
    {
        network = FindObjectOfType<NetworkManager>();

        direction = new Vector2(1f, 0.5f).normalized;
    }

    void Update()
    {
        if (network == null)
            return;

        // CLIENTE
        if (network.mode == NetworkManager.NetworkMode.Client)
        {
            Vector3 position = transform.position;

            position.x = Mathf.Lerp(
                position.x,
                network.ballX,
                15f * Time.deltaTime);

            position.y = Mathf.Lerp(
                position.y,
                network.ballY,
                15f * Time.deltaTime);

            transform.position = position;

            return;
        }

        // SERVIDOR
        transform.Translate(
            direction * speed * Time.deltaTime);

        CheckGoal();

        network.SendGameState(
            GameObject.Find("Player1").transform.position.y,
            GameObject.Find("Player2").transform.position.y,
            transform.position.x,
            transform.position.y,
            score1,
            score2);
    }

    void CheckGoal()
    {
        if (transform.position.x > 8f)
        {
            score1++;

            ResetBall(-1);
        }

        if (transform.position.x < -8f)
        {
            score2++;

            ResetBall(1);
        }
    }

    void ResetBall(int directionX)
    {
        transform.position = Vector3.zero;

        direction = new Vector2(
            directionX,
            Random.Range(-0.5f, 0.5f)).normalized;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (network.mode != NetworkManager.NetworkMode.Server)
            return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            direction.y *= -1;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            direction.x *= -1;
        }
    }
}