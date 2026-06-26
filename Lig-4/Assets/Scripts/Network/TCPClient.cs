using System.IO;
using System.Net.Sockets;

public class TCPClient
{
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

    public bool Conectado => client != null && client.Connected;

    public bool Conectar(string ip, int porta)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, porta);

            NetworkStream stream = client.GetStream();

            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Enviar(string mensagem)
    {
        if (!Conectado)
            return;

        writer.WriteLine(mensagem);
    }

    public string Receber()
    {
        if (!Conectado)
            return null;

        if (client.Available <= 0)
            return null;

        return reader.ReadLine();
    }

    public void Fechar()
    {
        if (reader != null)
            reader.Close();

        if (writer != null)
            writer.Close();

        if (client != null)
            client.Close();
    }
}