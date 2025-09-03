using UnityEngine;

public class Zone : MonoBehaviour
{
    [Header("Measure")]
    public int zoneCaptureCount = 0;
    public int playerCount;

    [Header("Conditions")]
    public bool isEnemyInZone;
    public bool enemyCapture;
    public bool isCaptured;
    public bool isPlayerInZone;
    public bool zoneContested;
    public bool gameWon = false;

    [Header("References")]
    public GameObject player;
    public GameObject enemy;

    public void Update()
    {
        if (gameWon) return; // If the game is won, skip further processing

        if (playerCount >= 2) //two players in the zone makes it contested
        {
            zoneContested = true;
        }
        else // If there are less than two players, the zone is not contested
        {
            zoneContested = false;
        }

        if (zoneContested) // If the zone is contested, reset the capture state
        {
            isCaptured = false;
            enemyCapture = false;
        }
        else if (isPlayerInZone) // If a player is in the zone and it's not contested
        {
            if (isEnemyInZone) // If an enemy is also in the zone
            {
                enemyCapture = true;
            }
            else // If no enemy is in the zone, the player captures it
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player1")) //The moment the players collider makes contact with the zone
        {
            isPlayerInZone = true;
            playerCount++; //to keep track of how many playyers are in the zone
        }
        else if (other.gameObject.CompareTag("Player2")) //The moment the players collider makes contact with the zone
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
