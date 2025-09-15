using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ZoneScore : MonoBehaviour
{
    public static ZoneScore instance;
    public TextMeshProUGUI zoneScoreText;
    public int zoneCaptureCount = 0;
    public bool gameWon = false;

    public void Awake()
    {
        instance = this;
    }
    
    public int scoreToWin = 3; // Score needed to win    
    void Start()
    {
        zoneScoreText.text = "Zones Captured: " + zoneCaptureCount.ToString();
    }

    public void AddZoneScore()
    {
        zoneCaptureCount++;
        zoneScoreText.text = "Zones Captured: " + zoneCaptureCount.ToString();
    }

    
    void Update()
    {
        if (zoneCaptureCount >= scoreToWin)
        {
            gameWon = true;
            SceneManager.LoadScene("Win Scene");
        }
    }
}
