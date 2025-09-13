using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{

    public Image healthBar;
    public Image[] healthPoints;
    public int playerIndex = 1; // Set this in inspector to match player (1 or 2)
    int currentHealth, maxHealth = 100;
    private float lerpSpeed;
    public Player player; // Reference to the Player script

    void Start()
    {
        // Find the player based on index
        FindPlayer();

        // If player not found, try again after a delay (in case players spawn later)
        if (player == null)
        {
            Invoke("FindPlayer", 1f);
        }
    }

    void FindPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            Player playerComponent = p.GetComponent<Player>();
            if (playerComponent != null && playerComponent.playerNumber == playerIndex)
            {
                player = playerComponent;

                // Subscribe to health changes
                player.OnHealthChanged += UpdateHealth;

                // Initialize health display
                currentHealth = player.GetCurrentHealth();
                maxHealth = player.GetMaxHealth();
                UpdateHealthBar();

                break;
            }
        }
    }

    void UpdateHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthPoints == null || healthPoints.Length == 0) return;

        for (int i = 0; i < healthPoints.Length; i++)
        {
            if (healthPoints[i] != null)
            {
                healthPoints[i].enabled = !DisplayHealthPoints(currentHealth, i);
            }
        }
    }

    public bool DisplayHealthPoints(float health, int pointNumber)
    {
        return ((pointNumber * 10) >= health);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealth;
        }
    }

    // public void Heal(float healPoints)
    // {
    //     if (health < maxHealth)
    //         health += healPoints;
    // }
}
