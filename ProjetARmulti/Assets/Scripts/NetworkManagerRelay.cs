using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
using HelloWorld;
using Unity.Services.Core;
using UnityEngine.SceneManagement;

public class NetworkManagerRelay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private GameObject roleSelectionPanel;
    [SerializeField] private GameObject connectionPanel;

    [Header("Scene Names")]
    [SerializeField] private string specialisteSceneName = "scene_Specialiste";
    [SerializeField] private string technicienSceneName = "scene_Technicien";

    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var playerObjects = FindObjectsOfType<NetworkObject>();
            foreach (var networkObject in playerObjects)
            {
                if (networkObject.OwnerClientId == clientId)
                {
                    networkObject.Despawn();
                }
            }
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

    public void ShowRoleSelection()
    {
        if (roleSelectionPanel != null)
        {
            roleSelectionPanel.SetActive(true);
        }
    }

    public void SelectRole(string role)
    {
        Debug.Log($"SelectRole appelé avec le rôle: {role}");

        PlayerRole selectedRole = (PlayerRole)System.Enum.Parse(typeof(PlayerRole), role);
        string sceneToLoad = selectedRole == PlayerRole.Specialiste
            ? specialisteSceneName
            : technicienSceneName;

        // Charger directement la scène
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);

        // Masquer le panneau de sélection de rôle
        if (roleSelectionPanel != null)
        {
            roleSelectionPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }
}
