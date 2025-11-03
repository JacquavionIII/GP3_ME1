using System.Net;
using System.Net.Sockets; // For UdpClient communication
using System.Text;
using System.Threading;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LanDiscovery : MonoBehaviour
{
    public int broadcastPort = 47777; // LAN discovery port where all the devices will communicate with each ther
    public string broadcastMessage = "NGO_HOST_HERE"; // Message to identify the host

    private UdpClient udpClient; // UDP client for listening to broadcasts
    private Thread listenThread;
    public string detectedHostIP;  // Detected host IP address

    private void Start() //Did this here instead of Awake to ensure other scripts have initialized first
    {
        detectedHostIP = null;
        StartListening();
    }

    private void OnDestroy()
    {
        listenThread?.Abort();  // Stop listening thread
        udpClient?.Close();  // Close UDP client
    }

    #region Host
    public void BroadcastHost()
    {
        ThreadPool.QueueUserWorkItem(_ =>  // Runs broadcasting in background so that the game doesnt freeze
        {
            using (var client = new UdpClient())
            {
                client.EnableBroadcast = true; // Crucial for broadcasting
                IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, broadcastPort); // Special broadcast address tht reaches all devices in the LAN
                byte[] data = Encoding.UTF8.GetBytes(broadcastMessage);

                while (true) //keeps broadcasting infinitely while true
                {
                    client.Send(data, data.Length, ep);
                    Thread.Sleep(1000); // send every 1 second
                }
            }
        });
    }
    #endregion

    #region Client
    private void StartListening()
    {
        listenThread = new Thread(() =>
        {
            udpClient = new UdpClient(broadcastPort); // This creates a scoket bound to the specified port
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, broadcastPort); // Listen to any IP address on the specified port

            while (true)
            {
                try  
                {
                    byte[] data = udpClient.Receive(ref remoteEP); // Blocking call that waits for incoming data
                    string message = Encoding.UTF8.GetString(data);
                    if (message == broadcastMessage)
                    {
                        detectedHostIP = remoteEP.Address.ToString(); // Store detected host IP
                        Debug.Log("Detected host: " + detectedHostIP);
                    }
                }
                catch { } //Catches any exceptions like when you're closing the UDP client
            }
        });
        listenThread.IsBackground = true; // Makes sure that the thread stops and also allows you to close the game
        listenThread.Start();
    }
    #endregion
}

