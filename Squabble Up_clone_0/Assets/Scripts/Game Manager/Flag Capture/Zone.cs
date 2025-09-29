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
    private int currentOwner = 0; // 0 = no player, 1 = Player1, 2 = Player2

    [Header("References")]
    public GameObject player;
    public GameObject enemy;
    public GameManager gm;
    public Image zoneDisplay; //tryin something... calling the UI image directly editing it
    public Color player1Colour = Color.blue;
    public Color player2Colour = Color.red;
    public Color contestedColour = Color.yellow;

    public void Update() //I made a lot of comments cause this sht genuinely confuses me when i need to read the logic behind the if statements since i made a lot of them
    {
        if (gameWon) return;

        // Handle zone contention
        zoneContested = (playerCount >= 2);

        // Handle ownership transitions and UI
        if (zoneContested)
        {
            HandleContestedState();
        }
        else if (isCaptured)
        {
            HandleCapturedState();
        }
        else
        {
            HandleNeutralState();
        }

        // Handle capture logic
        UpdateCaptureState();
    }

    private void HandleContestedState()
    {
        zoneDisplay.color = contestedColour;
        if (currentOwner != 0)
        {
            // Remove zone from previous owner when contested
            if (currentOwner == 1) gm.p1ZoneCount--;
            else if (currentOwner == 2) gm.p2ZoneCount--;
            currentOwner = 0;
        }
    }

    private void HandleCapturedState()
    {
        if (player1 && currentOwner != 1)
        {
            UpdateOwnership(1, player1Colour);
        }
        else if (player2 && currentOwner != 2)
        {
            UpdateOwnership(2, player2Colour);
        }
    }

    private void HandleNeutralState()
    {
        zoneDisplay.color = Color.green;
        if (currentOwner != 0)
        {
            // Remove zone from previous owner when neutral
            if (currentOwner == 1) gm.p1ZoneCount--;
            else if (currentOwner == 2) gm.p2ZoneCount--;
            currentOwner = 0;
        }
    }

    private void UpdateOwnership(int newOwner, Color color)
    {
        // Remove from previous owner
        if (currentOwner == 1) gm.p1ZoneCount--;
        else if (currentOwner == 2) gm.p2ZoneCount--;

        // Add to new owner
        if (newOwner == 1) gm.p1ZoneCount++;
        else if (newOwner == 2) gm.p2ZoneCount++;

        currentOwner = newOwner;
        zoneDisplay.color = color;
    }

    private void UpdateCaptureState()
    {
        if (zoneContested)
        {
            isCaptured = false;
            enemyCapture = false;
        }
        else if (isPlayerInZone)
        {
            enemyCapture = isEnemyInZone;
            isCaptured = !enemyCapture;
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
