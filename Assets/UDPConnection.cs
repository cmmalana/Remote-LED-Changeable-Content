using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnection : MonoBehaviour
{
    public MainScript mainScript;

    UdpClient receiverClient;
    Thread receiveThread;

    // Shared port for all senders/receivers
    const int port = 5005;

    private static readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    void Start()
    {
        // ✅ Allow multiple apps to bind the same port
        receiverClient = new UdpClient(AddressFamily.InterNetwork);
        receiverClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        receiverClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

        // Start receiver thread
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log($"UDP Receiver started on port {port}");
    }

    void Update()
    {
        // Process queued actions on Unity's main thread
        while (actionQueue.TryDequeue(out Action action))
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error executing queued action: " + ex);
            }
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            receiveThread?.Abort();
            receiverClient?.Close();
        }
        catch { }
    }

    // ✅ Send to everyone (broadcast)
    public void SendBroadcast(string message)
    {
        using (UdpClient senderClient = new UdpClient())
        {
            senderClient.EnableBroadcast = true;
            byte[] data = Encoding.UTF8.GetBytes(message);
            senderClient.Send(data, data.Length, "255.255.255.255", port);
            Debug.Log("Broadcast sent: " + message);
        }
    }

    // ✅ Send to specific IP (unicast)
    public void SendUnicast(string message, string targetIP)
    {
        using (UdpClient senderClient = new UdpClient())
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            senderClient.Send(data, data.Length, targetIP, port);
            Debug.Log($"Unicast sent to {targetIP}: {message}");
        }
    }

    // ✅ Receiver loop
    void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);

        while (true)
        {
            try
            {
                byte[] receivedData = receiverClient.Receive(ref remoteEP);
                string text = Encoding.UTF8.GetString(receivedData);
                Debug.Log($"Received from {remoteEP.Address}: {text}");

                // Example commands
                // if (text == "next")
                // {
                //     actionQueue.Enqueue(() => mainScript.NextVideo());
                // }
                // else if (text == "previous")
                // {
                //     actionQueue.Enqueue(() => mainScript.PreviousVideo());
                // }
            }
            catch (SocketException ex)
            {
                Debug.LogError("Receive error: " + ex);
                break;
            }
        }
    }
}
