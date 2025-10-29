using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{//this script is allowing the player/host to directly control their own transform without server authority
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
