using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ConnectUIScript : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private LanDiscovery lanDiscovery;

    public Transform connectUI;

    private void Start()
    {
        hostButton.onClick.AddListener(hostButtonOnClick);
        clientButton.onClick.AddListener(clientButtonOnClick);
    }

    public void hostButtonOnClick()
    {
        if (NetworkManager.Singleton.StartHost()) //originally had the StatHost by itsef, but running this through LanDiscovery helps the broadcasting process become more reliable
        {
            lanDiscovery.BroadcastHost(); // Thjis starts the broadcasting
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; //subscribing to know when the client connects
            CheckBeginOnline(); // Check if we can begin the online game especially if the host is count as P1
        }

    }

    public void clientButtonOnClick()
    {
        string ip;
        #if UNITY_EDITOR
        // When testing in Unity multiplayer play mode, force localhost
        ip = "127.0.0.1";

        #else
        // In actual LAN play, use discovered IP
        if (lanDiscovery.detectedHostIP == null)
        {
            Debug.LogError("No host detected on LAN.");
            return;
        }
        ip = lanDiscovery.detectedHostIP;
        #endif

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ip;

        if (NetworkManager.Singleton.StartClient())
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; //subscribing to know when the client connects
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        CheckBeginOnline();
    }

    private void CheckBeginOnline()
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            BeginOnline();
        }
    }
    
    private void BeginOnline()
    {
        //initially this was going to be used to do a scene swap, so that you can go to the online scene but I'll consider that for later...
        // Hide and lock the system cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        connectUI.gameObject.SetActive(false);
    }
}
