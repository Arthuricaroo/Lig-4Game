using UnityEngine;

public class ColunaClicavel : MonoBehaviour
{
    public int indiceColuna;
    public GameManager gameManager;

    void OnMouseDown()
    {
        gameManager.TentarJogar(indiceColuna, gameManager.JogadorAtual);
    }
}