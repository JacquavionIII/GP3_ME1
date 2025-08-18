using UnityEngine;

public class Zone : MonoBehaviour
{
    [Header("Measure")]
    public int zoneCaptureCount;
    public int playerCount;

    [Header("Conditions")]
    public bool isEnemyInZone;
    public bool enemyCapture;
    public bool isCaptured;
    public bool isPlayerInZone;
    public bool zoneContested;
    public bool gameWon;

    [Header("References")]
    public GameObject player;
    public GameObject enemy;

    public void Update()
    {
        if (gameWon) return; // If the game is won, skip further processing

        if (playerCount >= 2)
        {
            zoneContested = true;
        }
        else
        {
            zoneContested = false;
        }

        if (zoneContested)
        {
            isCaptured = false;
            enemyCapture = false;
        }
        else if (isPlayerInZone)
        {
            if (isEnemyInZone)
            {
                enemyCapture = true;
            }
            else
            {
                enemyCapture = false;
                isCaptured = true;
                zoneCaptureCount++;
            }
        }

        if (zoneCaptureCount >= 4)
        {
            gameWon = true;
            //then I'll add the game won logic here, I'll probably call a GameManager method
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player")) //The moment the players collider makes contact with the zone
        {
            isPlayerInZone = true;
            playerCount++; //to keep track of how many playyers are in the zone
        }

        if (other.gameObject.CompareTag("Enemy")) //The moment the enemies collider makes contact with the zone
        {
            isEnemyInZone = true;
        }
    }
}
