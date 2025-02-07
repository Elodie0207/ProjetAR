using HelloWorld;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ScriptLivre : NetworkBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Slider sliderPages;
    [SerializeField] private GameObject sliderCanvas;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("[ScriptLivre] OnNetworkSpawn appelé");
        
        // Configuration du slider
        if (sliderPages != null)
        {
            Debug.Log("Configuration du slider");
            sliderPages.minValue = 0;
            sliderPages.maxValue = pages.Length - 1;
            sliderPages.wholeNumbers = true;
            sliderPages.onValueChanged.AddListener(ChangerPage);
            
            // Afficher la première page au démarrage
            ChangerPage(0);
        }
        
        Invoke("CheckPlayerRole", 1f);
    }

    private void CheckPlayerRole()
    {
        Debug.Log("[ScriptLivre] CheckPlayerRole appelé");
        
        if (!IsClient)
        {
            Debug.Log("[ScriptLivre] N'est pas un client, arrêt de la vérification");
            return;
        }

        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayer != null)
        {
            Debug.Log("[ScriptLivre] LocalPlayer trouvé");
            var player = localPlayer.GetComponent<HelloWorldPlayer>();
            
            if (player != null)
            {
                Debug.Log($"[ScriptLivre] Player trouvé, IsOwner: {player.IsOwner}, Rôle actuel: {player.Role.Value}");
                
                if (player.IsOwner)
                {
                    player.Role.OnValueChanged += OnRoleChanged;
                    // Vérification immédiate du rôle
                    UpdateCanvasVisibility(player.Role.Value);
                }
            }
            else
            {
                Debug.LogError("[ScriptLivre] HelloWorldPlayer component non trouvé");
            }
        }
        else
        {
            Debug.LogError("[ScriptLivre] LocalPlayer non trouvé");
        }
    }

    private void UpdateCanvasVisibility(PlayerRole role)
    {
        Debug.Log($"[ScriptLivre] UpdateCanvasVisibility appelé avec le rôle: {role}");
        
        if (sliderCanvas != null)
        {
            bool shouldBeActive = role == PlayerRole.Specialiste;
            sliderCanvas.SetActive(shouldBeActive);
            Debug.Log($"[ScriptLivre] Canvas défini à {shouldBeActive} pour le rôle {role}");
        }
        else
        {
            Debug.LogError("[ScriptLivre] sliderCanvas est null!");
        }
    }

    private void OnRoleChanged(PlayerRole previousRole, PlayerRole newRole)
    {
        Debug.Log($"[ScriptLivre] OnRoleChanged appelé: {previousRole} -> {newRole}");
        UpdateCanvasVisibility(newRole);
    }

    private void ChangerPage(float valeurSlider)
    {
        Debug.Log($"Changement de page à l'index : {valeurSlider}");
        int pageIndex = Mathf.RoundToInt(valeurSlider);
        
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                bool shouldBeActive = i == pageIndex;
                pages[i].SetActive(shouldBeActive);
                Debug.Log($"Page {i} définie à {shouldBeActive}");
            }
        }
    }

    private void OnDestroy()
    {
        if (sliderPages != null)
        {
            sliderPages.onValueChanged.RemoveListener(ChangerPage);
        }

        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayer != null)
        {
            var player = localPlayer.GetComponent<HelloWorldPlayer>();
            if (player != null)
            {
                player.Role.OnValueChanged -= OnRoleChanged;
            }
        }
    }
}