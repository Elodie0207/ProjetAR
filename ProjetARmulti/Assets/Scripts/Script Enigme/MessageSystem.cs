using Unity.Netcode;
using UnityEngine;
using TMPro;
using HelloWorld;

public class MessageSystem : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageDisplay; 
    
    [SerializeField]
    private float messageDisplayTime = 10f;  
    
    private void Update()
    {
   
        if (IsOwner && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Espace appuyé, tentative d'envoi du message");
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (playerObject != null)
            {
                var player = playerObject.GetComponent<HelloWorldPlayer>();
                if (player.Role.Value != PlayerRole.None)
                {
                    SendMessageServerRpc(player.Role.Value);
                }
            }
        }
    }

    [ServerRpc]
    private void SendMessageServerRpc(PlayerRole senderRole, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Message envoyé depuis le serveur.");
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        ShowMessageClientRpc(senderClientId, senderRole);  // Envoi du message aux autres clients
    }

    [ClientRpc]
    private void ShowMessageClientRpc(ulong senderClientId, PlayerRole senderRole)
    {
        Debug.Log($"Message reçu sur le client {NetworkManager.Singleton.LocalClientId} du joueur {senderClientId}");
    
        // Ne montre pas le message au joueur qui l'a envoyé
        if (NetworkManager.Singleton.LocalClientId == senderClientId)
            return;

        // Affiche le message
        if (messageDisplay != null)
        {
            string roleText = senderRole.ToString();
            messageDisplay.text = $"Coucou ! Message du {roleText}";
            // Lance la coroutine pour faire disparaître le message après un délai
            StartCoroutine(HideMessageAfterDelay());
        }
    }

    private System.Collections.IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        if (messageDisplay != null)
        {
            messageDisplay.text = "";
        }
    }
}