using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour
{
    private void Start()
    {
        if (IsHost)
        {
            Material newMaterial = Resources.Load<Material>("Materials/ServerPlayerTexture");

            // Check if the material was loaded successfully
            if (newMaterial != null)
            {
                // Assign the material to the Player's Renderer
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = newMaterial;
                }
            }
            else
            {
                Debug.LogError("Material not found!");
            }
        }
    }

    void Update()
    {
        if (IsServer)
        {
            //  float theta = Time.frameCount / 10.0f;
            //transform.position = new Vector3((float)Math.Cos(theta), 0.0f, (float)Math.Sin(theta));

        }
    }
}