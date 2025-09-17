using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    //private bool hasScored = false;
    public bool player1; //doin this to check which player got the zone.
    public bool player2; //so we'll call these two to see when the zone has been captured.

    [Header("References")]
    public GameObject player;
    public GameObject enemy;
    public Image zoneDisplay; //tryin something... calling the UI image directly editing it
    public Color player1Colour = Color.blue;
    public Color player2Colour = Color.red;
    public Color contestedColour = Color.yellow;

    public void Update() //I made a lot of comments cause this sht genuinely confuses me when i need to read the logic behind the if statements since i made a lot of them
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

        // Always update color based on state
        if (zoneContested)
        {
            zoneDisplay.color = contestedColour;
        }
        else if (isCaptured)
        {
            if (player1)
            {
                zoneDisplay.color = player1Colour;
            }
            else if (player2)
            {
                zoneDisplay.color = player2Colour;
            }    
        }
        else
        {
            zoneDisplay.color = Color.green;
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
                // if (!hasScored) //this is to make sure this fucking score only goes up once
                // {
                //     ZoneScore.instance.AddZoneScore();
                //     hasScored = true;
                // } //Imma fucken tweak if this bitch doesnt doesn't score once...
            }
        }

        // if (isCaptured == true) //if it's captured then we'll set it to the player that captured it
        // {
        //     if (player1 == true)
        //     {
        //         zoneDisplay.color = player1Colour;
        //     }
        //     else if (player2 == true)
        //     {
        //         zoneDisplay.color = player2Colour;
        //     }
        // }
        // else
        // {
        //     zoneDisplay.color = Color.white;
        // }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player1")) //The moment the players collider makes contact with the zone
        {
            isPlayerInZone = true;
            playerCount++; //to keep track of how many playyers are in the zone
            player1 = true;
        }
        else if (collision.gameObject.CompareTag("Player2")) //The moment the players collider makes contact with the zone
        {
            isPlayerInZone = true;
            playerCount++; //to keep track of how many playyers are in the zone
            player2 = true;
        }

        if (collision.gameObject.CompareTag("Enemy")) //The moment the enemies collider makes contact with the zone
        {
            isEnemyInZone = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player1")) //The moment the players collider makes contact with the zone
        {
            playerCount--;
            player1 = false;
            if (playerCount <= 0)
            {
                isPlayerInZone = false;
            }

        }
        else if (collision.gameObject.CompareTag("Player2")) //The moment the players collider makes contact with the zone
        {
            playerCount--;
            player2 = false;
            if (playerCount <= 0)
            {
                isPlayerInZone = false;
            }
        }
        
        if (collision.gameObject.CompareTag("Enemy")) //The moment the enemies collider makes contact with the zone
        {
            isEnemyInZone = false;
        }
    }
}
