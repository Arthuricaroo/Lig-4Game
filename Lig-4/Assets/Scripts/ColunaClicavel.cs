using UnityEngine;

public class ColunaClicavel : MonoBehaviour
{
    public int indiceColuna;
    public GameManager gameManager;

    private void OnMouseDown()
    {
        gameManager.JogadaLocal(indiceColuna);
    }
}