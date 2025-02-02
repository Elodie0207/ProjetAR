using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;
using HelloWorld;
using Unity.Networking.Transport.Relay;

public class NetworkManagerRelay : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private GameObject roleSelectionPanel;
    [SerializeField] private GameObject connectionPanel;
    private NetworkManager networkManager;
    private UnityTransport transport;

    private async void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        transport = GetComponent<UnityTransport>();

        networkManager.OnClientConnectedCallback += OnClientConnected;

        if (roleSelectionPanel != null)
            roleSelectionPanel.SetActive(false);

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Quand le deuxième joueur se connecte
        if (networkManager.ConnectedClients.Count == 2)
        {
            // Cache seulement le panel de connexion
            connectionPanel.SetActive(false);
            // Si c'est le client qui vient de se connecter, montrer son panel de rôle
            if (networkManager.IsClient && !networkManager.IsHost)
            {
                ShowRoleSelection();
            }
        }
    }

    public async void StartHostWithRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = new RelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            networkManager.StartHost();

            if (codeText != null)
            {
                codeText.text = "Code : " + joinCode;
                Debug.Log("Code de connexion généré : " + joinCode);
            }

            // Montrer immédiatement le panel de rôle pour le host
            ShowRoleSelection();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinGameWithRelay()
    {
        try
        {
            string joinCode = joinInput.text;
            Debug.Log("Tentative de connexion avec le code : " + joinCode);

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            networkManager.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void ShowRoleSelection()
    {
        Debug.Log("Tentative d'affichage du panel de rôle");
        if (roleSelectionPanel != null)
        {
            roleSelectionPanel.SetActive(true);
            Debug.Log("Panel de sélection de rôle activé");
        }
        else
        {
            Debug.LogError("Role Selection Panel non assigné!");
        }
    }

    public void SelectRole(string role)
    {
        var playerObject = networkManager.SpawnManager.GetLocalPlayerObject();
        if (playerObject != null)
        {
            var player = playerObject.GetComponent<HelloWorldPlayer>();
            if (player != null)
            {
                PlayerRole selectedRole = (PlayerRole)System.Enum.Parse(typeof(PlayerRole), role);
                player.SetRoleServerRpc(selectedRole);
                roleSelectionPanel.SetActive(false); // Cache le panel de rôle après sélection
                Debug.Log($"Rôle {role} sélectionné");
            }
        }
    }

    private void OnDestroy()
    {
        if (networkManager != null)
            networkManager.OnClientConnectedCallback -= OnClientConnected;
    }
}