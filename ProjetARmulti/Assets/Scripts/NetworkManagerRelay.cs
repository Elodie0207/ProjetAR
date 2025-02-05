using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using TMPro;
using HelloWorld;
using Unity.Services.Core;
using UnityEngine.SceneManagement;
using UnityEditor;
using Unity.Networking.Transport.Relay;

public class NetworkManagerRelay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private GameObject roleSelectionPanel;
    [SerializeField] private GameObject connectionPanel;

    [Header("Scene References")]
    [SerializeField] private string specialisteSceneName = "scene_Specialiste";
    [SerializeField] private string technicienSceneName = "scene_Technicien";
    [SerializeField] private SceneAsset specialisteScene;
    [SerializeField] private SceneAsset technicienScene;

    private NetworkVariable<bool> clientConnected = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (specialisteScene != null)
            specialisteSceneName = specialisteScene.name;
        if (technicienScene != null)
            technicienSceneName = technicienScene.name;
    }

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
        Debug.Log($"SelectRole appelé avec le rôle: {role}");

        if (NetworkManager.Singleton != null)
        {
            PlayerRole selectedRole = (PlayerRole)System.Enum.Parse(typeof(PlayerRole), role);
            string sceneName = selectedRole == PlayerRole.Specialiste ?
                specialisteSceneName : technicienSceneName;

            Debug.Log($"Tentative de chargement de la scène: {sceneName}");

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                LoadSceneForRole(sceneName);
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                RequestSceneLoadServerRpc(sceneName);
            }

            roleSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("NetworkManager est null!");
        }
    }

    private void LoadSceneForRole(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            Debug.Log($"Chargement de la scène {sceneName} en cours...");
        }
        else
        {
            Debug.LogError($"La scène {sceneName} n'existe pas dans le build!");
            // Liste les scènes disponibles pour le debug
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                Debug.Log($"Scène disponible: {SceneUtility.GetScenePathByBuildIndex(i)}");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneLoadServerRpc(string sceneName, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Serveur reçoit demande de chargement pour scène: {sceneName}");
        LoadSceneForRole(sceneName);
    }
}