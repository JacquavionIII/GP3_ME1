using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public bool player1; // Is this camera for Player 1
    public bool player2; // Is this camera for Player 2

    public Player player;  // Reference to the Player script

    void Awake()
    {
        string parentTag = transform.parent.tag;

        if (parentTag == "Player1")
        {
            player1 = true;
            player2 = false;
        }
        else if (parentTag == "Player2")
        {
            player2 = true;
            player1 = false;
        }
        else
        {
            Debug.LogWarning("Parent GameObject is not tagged as Player1 or Player2.");
        }    
    }

    public void Update()
    {
        if (player1 == true)
        {
            gameObject.tag = "Player1Camera";
        }
        else if (player2 == true)
        {
            gameObject.tag = "Player2Camera";
        }
    }
    //gameObject.tag = "MainCamera";
}
