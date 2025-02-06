using Unity.Netcode;
using UnityEngine;
using Vuforia;

public class ARManager : NetworkBehaviour
{
    public GameObject arCamera;  // Référence à l'AR Camera de la scène
    
    void Start()
    {
        // Désactive la caméra AR au démarrage
        arCamera.SetActive(false);
        
        // S'abonne à l'événement de spawn du joueur local
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong id) =>
        {
            if (NetworkManager.Singleton.LocalClientId == id)
            {
                // Active la caméra AR uniquement pour le joueur local
                arCamera.SetActive(true);
                VuforiaBehaviour.Instance.enabled = true;
            }
        };
    }
}