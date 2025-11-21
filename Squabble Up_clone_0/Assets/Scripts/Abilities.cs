using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInput))]
public class Abilities : NetworkBehaviour
{
    [Header("References")]
    public Player player;
    public bool isPlayer1;
    public bool isPlayer2;
    public Transform healVFX;
    public Transform thunderVFX;
    public Transform burnVFX;
    public Transform freezeVFX;
    public Player opponent;

    [Header("Heal Ability Settings")]
    public float healAmount = 20f;
    public float healAbilityCooldown = 20f;
    public int healAbilityCount = 2;
    public bool healAblityUsed = false;
    public bool healDone = false;

    [Header("Thunder Ability Settings")]
    public float thunderStunDuration = 2f;
    public int thunderIntervals = 3;
    public bool isShocked = false;
    public float thunderIntervalDelay = 3f;
    public float thunderAbilityCooldown = 20f;
    public int thunderAbilityCount = 2;
    public bool thunderAblityUsed = false;
    public bool thunderDone = false;

    [Header("Burn Ability Settings")]
    public float burnDuration = 10f;
    public int burnDamagePerSecond = 2;
    public float burnAbilityCooldown = 20f;
    public int burnAbilityCount = 2;
    public bool burnAblityUsed = false;
    public bool burnDone = false;
    private Coroutine burnCoroutine;

    [Header("Freeze Ability Settings")]
    public float freezeDuration = 5f;
    public float freezeDamage = 10f;
    public float freezeAbilityCooldown = 20f;
    public int freezeAbilityCount = 2;
    public bool freezeAblityUsed = false;
    public bool freezeDone = false;

    [Header("Input Actions")]
    public InputAction healAction;         // Input action for movement
    public InputAction thunderAction;         // Input action for looking around
    public InputAction burnAction;         // Input action for jumping
    public InputAction freezeAction;  // Input action for attack

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();

        //Get the player's input actions
        var playerInput = GetComponent<PlayerInput>();
        healAction = playerInput.actions["Heal"];
        thunderAction = playerInput.actions["Thunder"];
        burnAction = playerInput.actions["Burn"];
        freezeAction = playerInput.actions["Freeze"];

        FindOpponent();
    }

    private void FindOpponent()
    {
        if (!IsOwner)
        {
            return;
        }

        Player[] allPlayers = FindObjectsOfType<Player>();
        foreach (Player p in allPlayers)
        {
            if (p != player && p.IsOwner == false) // This is the opponent
            {
                opponent = p;
                Debug.Log($"Found opponent: {p.gameObject.name}, isP1: {p.isP1}, isP2: {p.isP2}");
                break;
            }
        }
        
        // Alternative method using tags
        if (opponent == null)
        {
            if (player.isP1)
            {
                GameObject p2Object = GameObject.FindGameObjectWithTag("Player2");
                if (p2Object != null) opponent = p2Object.GetComponent<Player>();
            }
            else if (player.isP2)
            {
                GameObject p1Object = GameObject.FindGameObjectWithTag("Player1");
                if (p1Object != null) opponent = p1Object.GetComponent<Player>();
            }
        }
    }

    public void Update()
    {
        AbilitiesUsable();
    }

    public void AbilitiesUsable()
    {
        // if (thunderAbilityCount <= 0)
        // {
        //     thunderDone = true;
        //     if (thunderDone)
        //     {
        //         if (thunderAction != null) { if (enabled) thunderAction.Enable(); else thunderAction.Disable(); }
        //     }
        // }

        if (healAbilityCount <= 0)
        {
            healDone = true;
            if (freezeDone)
            {
                if (healAction != null) { if (enabled) healAction.Enable(); else healAction.Disable(); }
            }
        }

        // if (freezeAbilityCount <= 0)
        // {
        //     freezeDone = true;
        //     if (freezeDone)
        //     {
        //         if (freezeAction != null) { if (enabled) freezeAction.Enable(); else freezeAction.Disable(); }
        //     }
        // }
        
        // if (burnAbilityCount <= 0)
        // {
        //     burnDone = true;
        //     if (burnDone)
        //     {
        //        if (burnAction != null) { if (enabled) burnAction.Enable(); else burnAction.Disable(); }
        //     }
        // }
    }

    public void OnEnable()
    {
        healAction.performed += OnHeal;
        // thunderAction.performed += OnThunder;
        // burnAction.performed += OnBurn;
        // freezeAction.performed += OnFreeze;
    }

    public void OnDisable()
    {
        healAction.performed -= OnHeal;
        // thunderAction.performed -= OnThunder;
        // burnAction.performed -= OnBurn;
        // freezeAction.performed -= OnFreeze;
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        if (!context.performed || !IsOwner) return;
        Debug.Log("Heal ability activated!");
        HealAbility();
    }

    public void HealAbility()
    {
        player.TakeDamage(-20); //heals the player by 20 health points
        healAblityUsed = true;
        if (healVFX != null)
        {
            healVFX.gameObject.SetActive(true);
            Invoke(nameof(DisableHealVFX), 2.0f); // Disable after 1 second
        }
        healAbilityCount--;
        player.dmgDisplay.gameObject.SetActive(false);
    }

    private void DisableHealVFX()
    {
        if (healVFX != null)
        {
            healVFX.gameObject.SetActive(false);
        }
        healAblityUsed = false;
    }

    // public void OnThunder(InputAction.CallbackContext context)
    // {
    //     if (!context.performed || !IsOwner) return;
    //     Debug.Log("Thunder ability activated!");
    //     ThunderAbility();
    // }

    // public void ThunderAbility()
    // {
    //     if (opponent == null) 
    //     {
    //         FindOpponent();
    //         if (opponent == null) return;
    //     }
    
    //     thunderAblityUsed = true;
    
    //     // Show VFX on opponent
    //     if (thunderVFX != null)
    //     {
    //         thunderVFX.position = opponent.transform.position;
    //         thunderVFX.gameObject.SetActive(true);
    //     }

    //     // Stun the opponent
    //     opponent.DisableControls(thunderStunDuration);
    
    //     thunderAbilityCount--;
    //     Invoke(nameof(DisableThunderVFX), thunderStunDuration);
    // }

    // private void DisableThunderVFX()
    // {
    //     if (thunderVFX != null)
    //     {
    //         thunderVFX.gameObject.SetActive(false);
    //     }
    //     thunderAblityUsed = false;
    // }

    // public void OnBurn(InputAction.CallbackContext context)
    // {
    //     if (!context.performed || !IsOwner) return;
    //     Debug.Log("Burn ability activated!");
    //     BurnAbility();
    // }

    // public void BurnAbility()
    // {
    //     // I want to decrease the enemy's health over time

    //     if (opponent == null)
    //     {
    //         FindOpponent();
    //         if (opponent == null) return;
    //     }
        
    //     if (burnAbilityCount <= 0 || burnAblityUsed) return;
    //     burnAblityUsed = true;
    //     if (burnCoroutine != null)
    //     {
    //         StopCoroutine(burnCoroutine);
    //     }
    //     burnCoroutine = StartCoroutine(BurnCoroutine(burnDuration));

    //     if (burnVFX != null)
    //     {
    //         burnVFX.position = opponent.transform.position;
    //         burnVFX.gameObject.SetActive(true);
    //     }
        
    //     burnAbilityCount--;
        
    // }

    // private IEnumerator BurnCoroutine(float duration)
    // {
    //     float elapsed = 0f;
    //     while (elapsed < duration)
    //     {
    //         // apply damage once per second
    //         opponent.TakeDamage(burnDamagePerSecond);
    //         yield return new WaitForSeconds(burnDuration);
    //         elapsed += 1f;
    //     }

    //     // done
    //     Invoke(nameof(DisableBurnVFX), burnDuration);
    //     burnCoroutine = null;
    // }

    // public void DisableBurnVFX()
    // {
    //     if (burnVFX != null)
    //     {
    //         burnVFX.gameObject.SetActive(false);
    //     }
    //     burnAblityUsed = false;
    // }

    // public void OnFreeze(InputAction.CallbackContext context)
    // {
    //     if (!context.performed || !IsOwner) return;
    //     Debug.Log("Freeze ability activated!");
    //     FreezeAbility();
    // }

    // public void FreezeAbility()
    // {
    //     // I want to completely stop the enemy from moving for a few seconds and then and then damage them a bit.
    //     if (opponent == null)
    //     {
    //         FindOpponent();
    //         if (opponent == null) return;
    //     }
        
    //     opponent.DisableControls(freezeDuration);
    //     opponent.TakeDamage(10);

    //     if (freezeVFX != null)
    //     {
    //         freezeVFX.position = opponent.transform.position;
    //         freezeVFX.gameObject.SetActive(true);
    //     }
    
    //     freezeVFX.gameObject.SetActive(true);
    //     Invoke(nameof(DisableFreezeVFX), freezeDuration);
    //     freezeAbilityCount--;
    // }

    // private void DisableFreezeVFX()
    // {
    //     if (freezeVFX != null)
    //     {
    //         freezeVFX.gameObject.SetActive(false);
    //     }
    //     freezeAblityUsed = false;
    // }
}
