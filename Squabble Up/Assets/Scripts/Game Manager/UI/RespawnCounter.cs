using UnityEngine;
using TMPro;

public class RespawnCounter : MonoBehaviour
{
    public float respawnCounter = 5f;
    public Player player;
    public bool isCounting = false;
    public TextMeshProUGUI respawnText;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public void Update()
    {
        if ((player.isP1 && gameManager.p1RespawnCount > GameManager.MAX_RESPAWNS) ||
            (player.isP2 && gameManager.p2RespawnCount > GameManager.MAX_RESPAWNS))
        {
            respawnText.text = "No Respawns Left";
            return;
        }
        
        if (player.death && !isCounting)
        {
            respawnCounter = 5f;
            isCounting = true;
        }

        if (isCounting)
        {
            respawnCounter -= Time.deltaTime;
            respawnText.text = "Respawn: " + respawnCounter.ToString();
            if (respawnCounter <= 0f)
            {
                respawnCounter = 0f;
                isCounting = false;
                //Gonna make sure to call my respawn logic here when it works properly. (Update: it works now...)
                player.Respawn(player.spawnPoint.position);
            }
        }

        if (!player.death)
        {
            isCounting = false;
            respawnCounter = 5f; // Optional: reset when alive
        }
    }
}
