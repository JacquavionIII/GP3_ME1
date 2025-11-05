using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInput))]
public class Abilities : NetworkBehaviour
{
    [Header("References")]
    public Player player;
    public Transform healVFX;
    public Transform thunderVFX;
    public Transform burnVFX;
    public Transform freezeVFX;

    [Header("Ability Settings")]
    public float healAmount = 20f;
    public float thunderStunDuration = 2f;
    public int thunderIntervals = 3;
    public float thunderIntervalDelay = 3f;
    public float burnDuration = 10f;
    public float burnDamagePerSecond = 2f;
    public float freezeDuration = 5f;
    public float freezeDamage = 10f;
    public float abilityCooldown = 20f;

    [Header("Input Actions")]
    public InputAction healAction;         // Input action for movement
    public InputAction thunderAction;         // Input action for looking around
    public InputAction burnAction;         // Input action for jumping
    public InputAction freezeAction;  // Input action for attack

    public override void OnNetworkSpawn()
    {
        Player player = GetComponent<Player>();

        //Get the player's input actions
        var playerInput = GetComponent<PlayerInput>();
        healAction = playerInput.actions["Heal"];
        thunderAction = playerInput.actions["Thunder"];
        burnAction = playerInput.actions["Burn"];
        freezeAction = playerInput.actions["Freeze"];
    }

    public void OnEnable()
    {
        healAction.performed += OnHeal;
        thunderAction.performed += OnThunder;
        burnAction.performed += OnBurn;
        freezeAction.performed += OnFreeze;
    }

    public void OnDisable()
    {
        healAction.performed -= OnHeal;
        thunderAction.performed -= OnThunder;
        burnAction.performed -= OnBurn;
        freezeAction.performed -= OnFreeze;
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        Debug.Log("Heal ability activated!");
        HealAbility();
    }

    public void HealAbility()
    {
        player.TakeDamage(-20); //heals the player by 20 health points
    }

    public void OnThunder(InputAction.CallbackContext context)
    {
        Debug.Log("Thunder ability activated!");
        ThunderAbility();
    }

    public void ThunderAbility()
    {
        // I want to stun the player momentarily and implement a stun animation
    }

    public void OnBurn(InputAction.CallbackContext context)
    {
        Debug.Log("Burn ability activated!");
        BurnAbility();
    }

    public void BurnAbility()
    {
        // I want to decrease the enemy's health over time
    }

    public void OnFreeze(InputAction.CallbackContext context)
    {
        Debug.Log("Freeze ability activated!");
        FreezeAbility();
    }

    public void FreezeAbility()
    {
        // I want to completely stop the enemy from moving for a few seconds and then and then damage them a bit.
    }
}
