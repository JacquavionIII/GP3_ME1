using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    
    public Image healthBar;
    public Image[] healthPoints;
    int currentHealth, maxHealth = 100;
    private float lerpSpeed;
    public Player player; // Reference to the Player script

    void Start()
    {
        player.health = currentHealth;
        currentHealth = maxHealth;
    }

    void Update()
    {
        
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        lerpSpeed = 3f * Time.deltaTime;

        HealthBarFiller();
        
    }

    public void HealthBarFiller()
    {

        for (int i = 0; i < healthPoints.Length; i++)
        {
            healthPoints[i].enabled = !DisplayHealthPoints(currentHealth, i);
        }
    }

    public bool DisplayHealthPoints(float health, int pointNumber)
    {
        return ((pointNumber * 10) >= health);
    }

    // public void Heal(float healPoints)
    // {
    //     if (health < maxHealth)
    //         health += healPoints;
    // }
}
