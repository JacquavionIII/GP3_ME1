using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkAnimator : NetworkAnimator
{ //this allows us to modify the animator from the client side without server authority
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
