using UnityEngine;
using TMPro;

public class RespawnCounter : MonoBehaviour
{
    public float respawnCounter = 10f;
    public Player player;
    public bool isCounting = false;
    public TextMeshProUGUI respawnText;

    public void Update()
    {
        if (player.death && !isCounting)
        {
            respawnCounter = 10f;
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
                //Gonna make sure to call my respawn logic here when it works properly.
                player.Respawn(player.spawnPoint.position);
            }
        }

        if (!player.death)
        {
            isCounting = false;
            respawnCounter = 10f; // Optional: reset when alive
        }
    }
}
