using UnityEngine;
using UnityEngine.SceneManagement;

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
    private bool hasScored = false;

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
                if (!hasScored) //this is to make sure this fucking score only goes up once
                {
                    ZoneScore.instance.AddZoneScore();
                    hasScored = true;
                } //Imma fucken tweak if this bitch doesnt doesn't score once...
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player1")) //The moment the players collider makes contact with the zone
        {
            isPlayerInZone = true;
            playerCount++; //to keep track of how many playyers are in the zone
        }
        else if (collision.gameObject.CompareTag("Player2")) //The moment the players collider makes contact with the zone
        {
            isPlayerInZone = true;
            playerCount++; //to keep track of how many playyers are in the zone
        }

        if (collision.gameObject.CompareTag("Enemy")) //The moment the enemies collider makes contact with the zone
        {
            isEnemyInZone = true;
        }
    }
}
