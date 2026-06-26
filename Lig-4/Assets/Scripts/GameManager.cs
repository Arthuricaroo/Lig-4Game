using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public const int COLUNAS = 7;
    public const int LINHAS = 6;

    private int[,] tabuleiro = new int[COLUNAS, LINHAS];

    private int jogadorAtual = 1;
    private bool jogoAcabou = false;

    public TextMeshProUGUI textoTurno;
    public TextMeshProUGUI textoResultado;

    public TabuleiroVisual tabuleiroVisual;

    public GameObject prefabPecaAmarela;
    public GameObject prefabPecaVermelha;

    public int JogadorAtual => jogadorAtual;

    void Start()
    {
        for (int c = 0; c < COLUNAS; c++)
        {
            for (int l = 0; l < LINHAS; l++)
            {
                tabuleiro[c, l] = 0;
            }
        }

        textoTurno.text = "Vez do jogador " + jogadorAtual;
        textoResultado.text = "";
    }

 

    public void JogadaLocal(int coluna)
    {
        if (jogoAcabou)
            return;

        if (!TCPManager.Instance.MeuTurno)
            return;

        if (RealizarJogada(coluna, jogadorAtual))
        {
            TCPManager.Instance.EnviarJogada(coluna);
        }
    }


    public void JogadaRecebida(int coluna)
    {
        if (jogoAcabou)
            return;

        RealizarJogada(coluna, jogadorAtual);
    }


    private bool RealizarJogada(int coluna, int jogador)
    {
        if (coluna < 0 || coluna >= COLUNAS)
            return false;

        for (int l = 0; l < LINHAS; l++)
        {
            if (tabuleiro[coluna, l] == 0)
            {
                tabuleiro[coluna, l] = jogador;

                GameObject prefabEscolhido =
                    jogador == 1
                    ? prefabPecaAmarela
                    : prefabPecaVermelha;

                Instantiate(
                    prefabEscolhido,
                    tabuleiroVisual.posicoesCelulas[coluna, l],
                    Quaternion.identity
                );

                if (VerificarVitoria(coluna, l, jogador))
                {
                    jogoAcabou = true;
                    textoResultado.text = "Jogador " + jogador + " venceu!";
                }
                else if (TabuleiroCheio())
                {
                    jogoAcabou = true;
                    textoResultado.text = "Empate!";
                }
                else
                {
                    jogadorAtual = jogadorAtual == 1 ? 2 : 1;
                    textoTurno.text = "Vez do jogador " + jogadorAtual;
                }

                return true;
            }
        }

        return false;
    }


    private bool VerificarVitoria(int coluna, int linha, int jogador)
    {
        int[][] direcoes =
        {
            new int[]{1,0},
            new int[]{0,1},
            new int[]{1,1},
            new int[]{1,-1}
        };

        foreach (var dir in direcoes)
        {
            int count = 1;

            count += ContarNaDirecao(coluna, linha, dir[0], dir[1], jogador);
            count += ContarNaDirecao(coluna, linha, -dir[0], -dir[1], jogador);

            if (count >= 4)
                return true;
        }

        return false;
    }

    private int ContarNaDirecao(
        int coluna,
        int linha,
        int dc,
        int dl,
        int jogador)
    {
        int count = 0;

        int c = coluna + dc;
        int l = linha + dl;

        while (
            c >= 0 &&
            c < COLUNAS &&
            l >= 0 &&
            l < LINHAS &&
            tabuleiro[c, l] == jogador)
        {
            count++;

            c += dc;
            l += dl;
        }

        return count;
    }


    private bool TabuleiroCheio()
    {
        for (int c = 0; c < COLUNAS; c++)
        {
            for (int l = 0; l < LINHAS; l++)
            {
                if (tabuleiro[c, l] == 0)
                    return false;
            }
        }

        return true;
    }
}