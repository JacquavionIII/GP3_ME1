using UnityEngine;
using System.Collections;
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
    }

    public void Update()
    {
        AbilitiesUsable();
    }

    public void AbilitiesUsable()
    {
        if (thunderAbilityCount <= 0)
        {
            thunderDone = true;
            if (thunderDone)
            {
                if (thunderAction != null) { if (enabled) thunderAction.Enable(); else thunderAction.Disable(); }
            }
        }

        if (healAbilityCount <= 0)
        {
            healDone = true;
            if (freezeDone)
            {
                if (healAction != null) { if (enabled) healAction.Enable(); else healAction.Disable(); }
            }
        }

        if (freezeAbilityCount <= 0)
        {
            freezeDone = true;
            if (freezeDone)
            {
                if (freezeAction != null) { if (enabled) freezeAction.Enable(); else freezeAction.Disable(); }
            }
        }
        
        if (burnAbilityCount <= 0)
        {
            burnDone = true;
            if (burnDone)
            {
               if (burnAction != null) { if (enabled) burnAction.Enable(); else burnAction.Disable(); }
            }
        }
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

    public void OnThunder(InputAction.CallbackContext context)
    {
        Debug.Log("Thunder ability activated!");
        ThunderAbility();
    }

    public void ThunderAbility()
    {
        thunderAblityUsed = true;
        // I want to stun the player momentarily and implement a stun animation
        if (thunderVFX != null)
        {
            thunderVFX.gameObject.SetActive(true);
            
        }

        if (thunderAblityUsed && !isShocked && thunderIntervals == 3)
        {
            player.DisableControls(thunderStunDuration);
            isShocked = true;
        }

        thunderIntervalDelay -= Time.deltaTime;
        if (thunderIntervalDelay <= 0 && thunderIntervals > 0)
        {
            // Stun logic here
            thunderIntervals--;
            thunderIntervalDelay = 3f; // Reset delay for next interval
        }

        if (thunderIntervals == 0)
        {
            // Reset intervals for next use
            thunderIntervals = 3;
            thunderAbilityCount--;
            Invoke(nameof(DisableThunderVFX), thunderStunDuration);
        }
        thunderAbilityCount--;
    }

    private void DisableThunderVFX()
    {
        if (thunderVFX != null)
        {
            thunderVFX.gameObject.SetActive(false);
        }
        thunderAblityUsed = false;
    }

    public void OnBurn(InputAction.CallbackContext context)
    {
        Debug.Log("Burn ability activated!");
        BurnAbility();
    }

    public void BurnAbility()
    {
        // I want to decrease the enemy's health over time
        if (burnAbilityCount <= 0 || burnAblityUsed) return;
        burnAblityUsed = true;
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
        burnCoroutine = StartCoroutine(BurnCoroutine(burnDuration));
        burnAbilityCount--;
        
    }

    private IEnumerator BurnCoroutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // apply damage once per second
            player.TakeDamage(burnDamagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        // done
        Invoke(nameof(DisableBurnVFX), burnDuration);
        burnCoroutine = null;
    }

    public void DisableBurnVFX()
    {
        if (burnVFX != null)
        {
            burnVFX.gameObject.SetActive(false);
        }
        burnAblityUsed = false;
    }

    public void OnFreeze(InputAction.CallbackContext context)
    {
        Debug.Log("Freeze ability activated!");
        FreezeAbility();
    }

    public void FreezeAbility()
    {
        // I want to completely stop the enemy from moving for a few seconds and then and then damage them a bit.
        player.DisableControls(freezeDuration);
        player.TakeDamage(10);
        freezeVFX.gameObject.SetActive(true);
        Invoke(nameof(DisableFreezeVFX), freezeDuration);
        freezeAbilityCount--;
    }

    private void DisableFreezeVFX()
    {
        if (freezeVFX != null)
        {
            freezeVFX.gameObject.SetActive(false);
        }
        freezeAblityUsed = false;
    }
}
