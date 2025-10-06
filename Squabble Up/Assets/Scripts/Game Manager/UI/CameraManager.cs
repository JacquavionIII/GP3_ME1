using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    public bool player1; // Is this camera for Player 1
    public bool player2; // Is this camera for Player 2

    // Layer indices (I need to match the layers set up in Unity)
    public int player1UILayer = 8;
    public int player2UILayer = 9;

    private Camera cam; // Reference to the Camera component
    private string player1CanvasTag = "Player1UI";  // Tag for Player 1's UI canvas
    private string player2CanvasTag = "Player2UI";  // Tag for Player 2's UI canvas

    // protected override void OnStartAuthority()
    // {
    //     enabled = true; // Enable this script only for the local player's camera
    // } 

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraManager requires a Camera component!");
            return;
        }

        string parentTag = transform.parent.tag;  //Checking the parent tag, since I made sure to edit the player tags on instantiation in the player script

        if (parentTag == "Player1")
        {
            player1 = true;
            player2 = false;
            gameObject.tag = "Player1Camera";
        }
        else if (parentTag == "Player2")
        {
            player2 = true;
            player1 = false;
            gameObject.tag = "Player2Camera";
        }
        else
        {
            Debug.LogWarning("Parent GameObject is not tagged as Player1 or Player2."); //doing this to check if the camera is assigned to a player
        }

        SetupCameraLayers(); // // Setup camera and UI layers based on which player this camera belongs to
    }

    void SetupCameraLayers() //Sets up the camera layers based on which player this camera belongs to
    {
        if (player1)
        {
            SetupPlayerCamera(player1UILayer, player1CanvasTag, "Player1"); // Configuring this camera for Player 1
        }
        else if (player2)
        {
            SetupPlayerCamera(player2UILayer, player2CanvasTag, "Player2");
        }
        else
        {
            Debug.LogWarning("Camera is not assigned to any player. Using default settings.");
        }
    }

    void SetupPlayerCamera(int uiLayer, string canvasTag, string playerName) //Configures a camera for a specific player
    {
        GameObject playerCanvas = GameObject.FindWithTag(canvasTag); // Find the UI canvas for this player using the specified tag

        if (playerCanvas != null)
        {
            // Set the canvas to the appropriate layer
            SetLayerRecursively(playerCanvas, uiLayer);
            Debug.Log($"Assigned {playerName} UI to layer {uiLayer}");

            cam.cullingMask |= (1 << uiLayer); // Configure camera to render this UI layer to the culling mask
            // The |= operator adds the layer to the existing culling mask
        }
        else
        {
            Debug.LogWarning($"No UI canvas found with tag {canvasTag} for {playerName}");
        }
        
        // Ensure the camera doesn't render the other player's UI layer
        int otherUILayer = player1 ? player2UILayer : player1UILayer;
        cam.cullingMask &= ~(1 << otherUILayer);  // The &= ~ operator removes the layer from the culling mask
        
    }

    // Recursively set layer for all children of a GameObject
    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        
        obj.layer = layer;  //Set the layer for this GameObject
        
        foreach (Transform child in obj.transform) // Recursively set layers for all children
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // Public method to manually refresh layers if needed
    public void RefreshCameraLayers()
    {
        SetupCameraLayers();
    }
}