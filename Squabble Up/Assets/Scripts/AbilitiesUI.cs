using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class AbilitiesUI : NetworkBehaviour
{
    [Header("Player Stuff")]
    public Player player;
    public Abilities abilities;

    [Header("Heal")]
    public Transform healCD1;
    public Transform healCD2;
    public TextMeshProUGUI healCDText1;
    public TextMeshProUGUI healCDText2;

    // [Header("Thunder")]
    // public Transform thunderCD1;
    // public Transform thunderCD2;
    // public TextMeshProUGUI thunderCDText1;
    // public TextMeshProUGUI thunderCDText2;

    // [Header("Burn")]
    // public Transform burnCD1;
    // public Transform burnCD2;
    // public TextMeshProUGUI burnCDText1;
    // public TextMeshProUGUI burnCDText2;

    // [Header("Freeze")]
    // public Transform freezeCD1;
    // public Transform freezeCD2;
    // public TextMeshProUGUI freezeCDText1;
    // public TextMeshProUGUI freezeCDText2;

    // runtime coroutine handles to avoid duplicate countdowns
    private Coroutine healCoroutine;
    // private Coroutine thunderCoroutine;
    // private Coroutine burnCoroutine;
    // private Coroutine freezeCoroutine;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        
        //heal stuff converting the the text to string
        healCDText1.text = abilities.healAbilityCooldown.ToString();
        healCDText2.text = abilities.healAbilityCooldown.ToString();

        //thunder stuff
        // thunderCDText1.text = abilities.thunderAbilityCooldown.ToString();
        // thunderCDText2.text = abilities.thunderAbilityCooldown.ToString();

        // //burn stuff
        // burnCDText1.text = abilities.burnAbilityCooldown.ToString();
        // burnCDText2.text = abilities.burnAbilityCooldown.ToString();

        // //freeze stuff
        // freezeCDText1.text = abilities.freezeAbilityCooldown.ToString();
        // freezeCDText2.text = abilities.freezeAbilityCooldown.ToString();

    }

    // public void HealDis()
    // {
    //     if (abilities.healAblityUsed && player.isP1)
    //     {
    //         healCD1.gameObject.SetActive(true);
    //         healCDText1.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         healCD1.gameObject.SetActive(false);
    //         healCDText1.gameObject.SetActive(false);
    //     }

    //     if (abilities.healAblityUsed && player.isP2)
    //     {
    //         healCD2.gameObject.SetActive(true);
    //         healCDText2.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         healCD2.gameObject.SetActive(false);
    //         healCDText2.gameObject.SetActive(false);
    //     }
    // }

    // public void ThunDis()
    // {
    //     if (abilities.thunderAblityUsed && player.isP1)
    //     {
    //         thunderCD1.gameObject.SetActive(true);
    //         thunderCDText1.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         thunderCD1.gameObject.SetActive(false);
    //         thunderCDText1.gameObject.SetActive(false);
    //     }

    //     if (abilities.thunderAblityUsed && player.isP2)
    //     {
    //         thunderCD2.gameObject.SetActive(true);
    //         thunderCDText2.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         thunderCD2.gameObject.SetActive(false);
    //         thunderCDText2.gameObject.SetActive(false);
    //     }
    // }

    // public void BurnDis()
    // {
    //     if (abilities.burnAblityUsed && player.isP1)
    //     {
    //         burnCD1.gameObject.SetActive(true);
    //         burnCDText1.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         burnCD1.gameObject.SetActive(false);
    //         burnCDText1.gameObject.SetActive(false);
    //     }

    //     if (abilities.burnAblityUsed && player.isP2)
    //     {
    //         burnCD2.gameObject.SetActive(true);
    //         burnCDText2.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         burnCD2.gameObject.SetActive(false);
    //         burnCDText2.gameObject.SetActive(false);
    //     }
    // }

    // public void FreezeDis()
    // {
    //     if (abilities.freezeAblityUsed && player.isP1)
    //     {
    //         freezeCD1.gameObject.SetActive(true);
    //         freezeCDText1.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         freezeCD1.gameObject.SetActive(false);
    //         freezeCDText1.gameObject.SetActive(false);
    //     }

    //     if (abilities.freezeAblityUsed && player.isP2)
    //     {
    //         freezeCD2.gameObject.SetActive(true);
    //         freezeCDText2.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         freezeCD2.gameObject.SetActive(false);
    //         freezeCDText2.gameObject.SetActive(false);
    //     }
    // }
    
    // Call these methods when the corresponding ability is used.
    // They will start a UI countdown and hide the UI when the timer reaches 0.

    public void HealDis()
    {
        if (!IsOwner || abilities == null || player == null) return;

        float duration = abilities.healAbilityCooldown;
        // decide which UI to affect based on player slot
        if (player.isP1)
        {
            if (healCoroutine != null) StopCoroutine(healCoroutine);
            healCoroutine = StartCoroutine(CountdownCD(healCD1, healCDText1, duration));
        }
        else if (player.isP2)
        {
            if (healCoroutine != null) StopCoroutine(healCoroutine);
            healCoroutine = StartCoroutine(CountdownCD(healCD2, healCDText2, duration));
        }
    }

    // public void ThunDis()
    // {
    //     if (!IsOwner || abilities == null || player == null) return;

    //     float duration = abilities.thunderAbilityCooldown;
    //     if (player.isP1)
    //     {
    //         if (thunderCoroutine != null) StopCoroutine(thunderCoroutine);
    //         thunderCoroutine = StartCoroutine(CountdownCD(thunderCD1, thunderCDText1, duration));
    //     }
    //     else if (player.isP2)
    //     {
    //         if (thunderCoroutine != null) StopCoroutine(thunderCoroutine);
    //         thunderCoroutine = StartCoroutine(CountdownCD(thunderCD2, thunderCDText2, duration));
    //     }
    // }

    // public void BurnDis()
    // {
    //     if (!IsOwner || abilities == null || player == null) return;

    //     float duration = abilities.burnAbilityCooldown;
    //     if (player.isP1)
    //     {
    //         if (burnCoroutine != null) StopCoroutine(burnCoroutine);
    //         burnCoroutine = StartCoroutine(CountdownCD(burnCD1, burnCDText1, duration));
    //     }
    //     else if (player.isP2)
    //     {
    //         if (burnCoroutine != null) StopCoroutine(burnCoroutine);
    //         burnCoroutine = StartCoroutine(CountdownCD(burnCD2, burnCDText2, duration));
    //     }
    // }

    // public void FreezeDis()
    // {
    //     if (!IsOwner || abilities == null || player == null) return;

    //     float duration = abilities.freezeAbilityCooldown;
    //     if (player.isP1)
    //     {
    //         if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
    //         freezeCoroutine = StartCoroutine(CountdownCD(freezeCD1, freezeCDText1, duration));
    //     }
    //     else if (player.isP2)
    //     {
    //         if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
    //         freezeCoroutine = StartCoroutine(CountdownCD(freezeCD2, freezeCDText2, duration));
        //}
    //}

    // Generic countdown coroutine: shows the CD object, updates text each frame, hides when <= 0
    private System.Collections.IEnumerator CountdownCD(Transform cdTransform, TextMeshProUGUI cdText, float duration)
    {
        if (cdTransform == null || cdText == null) yield break;

        cdTransform.gameObject.SetActive(true);
        cdText.gameObject.SetActive(true);

        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            cdText.text = Mathf.Max(0f, remaining).ToString("F1");
            yield return null;
        }

        // ensure 0 display, then hide UI
        cdText.text = "0.0";
        cdTransform.gameObject.SetActive(false);
        cdText.gameObject.SetActive(false);
    }

}
