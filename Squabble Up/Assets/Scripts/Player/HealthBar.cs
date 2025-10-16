using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class HealthBar : NetworkBehaviour
{
    [Header("Health Bar Settings")]
    public Image healthBar;
    public int playerIndex = 1; // Set this in inspector to match player (1 or 2)
    
    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float colorChangeThreshold = 0.3f;
    
    [Header("Player-Data Settings")]
    private int currentHealth, maxHealth = 100;
    private float lerpSpeed = 3f;
    public Player player;
    private bool playerFound = false;

    void Start()
    {
        // Initial health bar setup
        if (healthBar != null)
        {
            healthBar.type = Image.Type.Filled;
            healthBar.fillMethod = Image.FillMethod.Horizontal;
            healthBar.fillAmount = 1f;
            healthBar.color = fullHealthColor;
        }
        
        // Delay initial player finding to ensure players are spawned
        Invoke("FindPlayer", 0.5f);
    }

    void Update()
    {
        // If player not found yet, keep trying
        if (!playerFound)
        {
            FindPlayer();
        }
        
        // Smooth health bar updates
        if (playerFound)
        {
            UpdateHealthBar();
        }
    }

    void FindPlayer()
    {
        // Use the specific player tag based on playerIndex
        string targetTag = "Player" + playerIndex;
        GameObject playerObject = GameObject.FindWithTag(targetTag);
        
        if (playerObject != null)
        {
            Player playerComponent = playerObject.GetComponent<Player>();
            if (playerComponent != null)
            {
                player = playerComponent;
                playerFound = true;
                
                // Unsubscribe first to avoid duplicate subscriptions
                player.OnHealthChanged -= UpdateHealth;
                // Subscribe to health changes
                player.OnHealthChanged += UpdateHealth;
                
                // Initialize health display
                currentHealth = player.GetCurrentHealth();
                maxHealth = player.GetMaxHealth();
                
                // Force immediate update
                UpdateHealth(currentHealth, maxHealth);
                
                Debug.Log($"Successfully connected to {targetTag} - Health: {currentHealth}/{maxHealth}");
            }
        }
        else
        {
            Debug.LogWarning($"{targetTag} not found. Will retry...");
        }
    }

    void UpdateHealth(int current, int max)
    {
        Debug.Log($"Health update received: {current}/{max}");
        currentHealth = current;
        maxHealth = max;
        HealthColour();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null && maxHealth > 0)
        {
            float targetFillAmount = (float)currentHealth / maxHealth;
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFillAmount, lerpSpeed * Time.deltaTime);
        }
    }

    void HealthColour()
    {
        if (healthBar != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            
            if (healthPercentage <= colorChangeThreshold)
            {
                float t = healthPercentage / colorChangeThreshold;
                healthBar.color = Color.Lerp(lowHealthColor, fullHealthColor, t);
            }
            else
            {
                healthBar.color = fullHealthColor;
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealth;
        }
    }
}//fuck C#, fuck any form of code cause i spent like 2-3hours on one line of code. Fuck you and fuck this, piece of fucking shit.