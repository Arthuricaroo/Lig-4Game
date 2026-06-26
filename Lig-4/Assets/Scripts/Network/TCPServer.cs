using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityEngine;

public class TCPServer
{
    TcpListener listener;
    TcpClient client;

    StreamReader reader;
    StreamWriter writer;

    Thread thread;

    public bool conectado = false;

    public void StartServer(int porta)
    {
        thread = new Thread(() =>
        {
            listener = new TcpListener(IPAddress.Any, porta);
            listener.Start();

            client = listener.AcceptTcpClient();

            NetworkStream stream = client.GetStream();

            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            writer.AutoFlush = true;

            conectado = true;
        });

        thread.IsBackground = true;
        thread.Start();
    }

    public void Send(string msg)
    {
        if (conectado)
            writer.WriteLine(msg);
    }

    public string Receive()
    {
        if (conectado && client.Available > 0)
            return reader.ReadLine();

        return null;
    }

    public void Fechar()
{
    if (reader != null)
        reader.Close();

    if (writer != null)
        writer.Close();

    if (client != null)
        client.Close();

    if (listener != null)
        listener.Stop();
}
}