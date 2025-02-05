using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;
using HelloWorld;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay.Models;

public class NetworkManagerRelay : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private GameObject roleSelectionPanel;
    [SerializeField] private GameObject connectionPanel;

    public void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            connectionPanel.SetActive(false);
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            ShowRoleSelectionClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    private void ShowRoleSelectionClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (roleSelectionPanel != null)
        {
            roleSelectionPanel.SetActive(true);
            Debug.Log("Panel de sélection de rôle activé");
        }
    }

    public async void StartHostWithRelay()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();

            if (codeText != null)
            {
                codeText.text = "Code : " + joinCode;
            }
            ShowRoleSelection();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Relay start error: {e.Message}");
        }
    }

    public void ShowRoleSelection()
    {
        if (roleSelectionPanel != null)
        {
            roleSelectionPanel.SetActive(true);
        }
    }

    public async void JoinGameWithRelay()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            string joinCode = joinInput.text;
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            connectionPanel.SetActive(false);
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Relay join error: {e.Message}");
        }
    }

    public void SelectRole(string role)
    {
        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObject != null)
        {
            var player = playerObject.GetComponent<HelloWorldPlayer>();
            if (player != null)
            {
                PlayerRole selectedRole = (PlayerRole)System.Enum.Parse(typeof(PlayerRole), role);
                player.SetRoleServerRpc(selectedRole);
                roleSelectionPanel.SetActive(false);
            }
        }
    }
}