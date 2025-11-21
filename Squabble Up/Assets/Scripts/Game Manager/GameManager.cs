using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("ZoneCapture and Win Logic")]
    public Player player1;
    public Player player2;
    public Zone zone;
    public int p1ZoneCount = 0;
    public int p2ZoneCount = 0;
    public int zoneWin = 4; //zone count needed to win

    [Header("Respawn Stuff")]
    public int p1RespawnCount = 0;
    public int p2RespawnCount = 0;
    public const int MAX_RESPAWNS = 2;
    private bool gameEnded = false;

    [Header("Other Stuff")]
    AudioManager audioManager;

    void Start()
    {
        // Find both players if not assigned
        if (player1 == null || player2 == null)
        {
            FindPlayers();
        }

        // Subscribe to death events
        if (player1 != null) player1.OnPlayerDeath += HandlePlayerDeath;
        if (player2 != null) player2.OnPlayerDeath += HandlePlayerDeath;
    }

    void FindPlayers()
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            if (player.isP1) player1 = player;
            else if (player.isP2) player2 = player;
        }
    }

    void HandlePlayerDeath(int playerNumber)
    {
        if (gameEnded) 
        {
            Cursor.visible = true;
            return;
        }

        // Increment the respawn count for the player who died
        if (playerNumber == 1)
        {
            p1RespawnCount++;
            Debug.Log("Player 1 death count: " + p1RespawnCount);

            // Check if player 1 has exceeded respawn limit
            if (p1RespawnCount > MAX_RESPAWNS)
            {
                Player2Wins();
                return;
            }
        }
        else if (playerNumber == 2)
        {
            p2RespawnCount++;
            Debug.Log("Player 2 death count: " + p2RespawnCount);

            // Check if player 2 has exceeded respawn limit
            if (p2RespawnCount > MAX_RESPAWNS)
            {
                Player1Wins();
                return;
            }
        }
    }

    void Update()
    {
        if (gameEnded) return; //to make sure that the game is constantly checking the value of this

        if (p1ZoneCount >= zoneWin)
        {
            Player1Wins();
            Cursor.visible = true;
        }
        else if (p2ZoneCount >= zoneWin)
        {
            Player2Wins();
            Cursor.visible = true;
        }
    }

    void Player1Wins()
    {
        gameEnded = true;
        print("Player 1 has won");
        SceneManager.LoadScene("P1Wins");
        audioManager.gameIsWon = true;
        Cursor.visible = true;
    }

    void Player2Wins()
    {
        gameEnded = true;
        print("Player 2 has won");
        SceneManager.LoadScene("P2Wins");
        audioManager.gameIsWon = true;
    }

    public override void OnDestroy()
    {
        // Unsubscribe from the events to stop the memory from cooking itself, optimisation yay.... kill me now
        if (player1 != null) player1.OnPlayerDeath -= HandlePlayerDeath;
        if (player2 != null) player2.OnPlayerDeath -= HandlePlayerDeath;
    }
}
