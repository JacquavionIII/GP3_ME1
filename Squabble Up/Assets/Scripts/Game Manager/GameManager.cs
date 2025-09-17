using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Zone zone;
    public int p1ZoneCount = 0;
    public int p2ZoneCount = 0;
    public int zoneWin = 4;

    void Update()
    {
        if (p1ZoneCount >= zoneWin)
        {
            print("Player 1 has won");
            SceneManager.LoadScene("P1Wins");
        }
        else if (p2ZoneCount >= zoneWin)
        {
            print("Player 2 has won");
            SceneManager.LoadScene("P2Wins");
        }
    }
}
