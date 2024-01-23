using System;
using System.Net;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Sockets;

public static class Dancer01Server
{
    private static TcpListener tcpListener;
    private static TcpClient tcpClient;
    private static Thread listenerThread;

    public static NetworkData networkData;
    public static bool breakThread = false;

    public static void Connect()
    {
        listenerThread = new(new ThreadStart(Listen))
        {
            IsBackground = true
        };
        listenerThread.Start();
    }

    private static void Listen()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse("192.168.1.2"), 13333);
            tcpListener.Start();
            byte[] bytes = new byte[100];
            while (true)
            {
                using (tcpClient = tcpListener.AcceptTcpClient())
                {
                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            if (breakThread) { break; }
                            byte[] incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            string clientMessage = Encoding.UTF8.GetString(incommingData);
                            try
                            {
                                networkData = JsonConvert.DeserializeObject<NetworkData>(clientMessage.Replace("*", ""));
                            }
                            catch { }
                        }
                    }
                }
                if (breakThread) { break; }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("SocketException " + socketException.ToString());
        }
    }
}

public struct NetworkData
{
    public float x;
    public float y;
    public float z;
    public NetworkAction action;
}

public enum NetworkAction
{
    None, Enter
}