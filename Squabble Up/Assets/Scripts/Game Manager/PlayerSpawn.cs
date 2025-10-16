using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerSpawn : NetworkBehaviour
{
    public Transform[] SpawnPoints;
    private int playerCount;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = SpawnPoints[playerCount].transform.position;
        playerCount++;
    }
}
