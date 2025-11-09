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
    public Transform healCD;
    public Text healCDText;

    [Header("Thunder")]
    public Transform thunderCD;
    public Text thunderCDText;

    // Update is called once per frame
    void Update()
    {
        
    }
}
