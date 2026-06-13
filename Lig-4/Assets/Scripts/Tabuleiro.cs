using UnityEngine;

public class TabuleiroVisual : MonoBehaviour
{
    public GameObject prefabCelulaVazia;
    public float espacamento = 1.1f;

    public Vector3[,] posicoesCelulas = new Vector3[GameManager.COLUNAS, GameManager.LINHAS];

    void Awake()
    {
        for (int c = 0; c < GameManager.COLUNAS; c++)
        {
            for (int l = 0; l < GameManager.LINHAS; l++)
            {
                Vector3 pos = transform.position + new Vector3(c * espacamento, l * espacamento, 0);
                posicoesCelulas[c, l] = pos;

                Instantiate(prefabCelulaVazia, pos, Quaternion.identity, transform);
            }
        }
    }
}