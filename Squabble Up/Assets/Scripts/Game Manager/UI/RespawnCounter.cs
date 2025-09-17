using UnityEngine;
using TMPro;

public class RespawnCounter : MonoBehaviour
{
    public float respawnCounter = 5f;
    public Player player;
    public bool isCounting = false;
    public TextMeshProUGUI respawnText;

    public void Update()
    {
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
