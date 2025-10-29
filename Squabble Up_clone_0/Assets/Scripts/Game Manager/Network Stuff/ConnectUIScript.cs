using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectUIScript : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Start()
    {
        hostButton.onClick.AddListener(hostButtonOnClick);
        clientButton.onClick.AddListener(clientButtonOnClick);
    }

    public void hostButtonOnClick()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void clientButtonOnClick()
    {
        NetworkManager.Singleton.StartClient();
    }

}
