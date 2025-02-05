using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Vuforia;
using HelloWorld;

public class ARSceneManager : NetworkBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string specialisteSceneName = "SpecialisteScene";
    [SerializeField] private string technicienSceneName = "TechnicienScene";

    [Header("AR Setup")]
    [SerializeField] private VuforiaBehaviour vuforiaBehaviour;

    private NetworkVariable<PlayerRole> playerRole = new NetworkVariable<PlayerRole>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        if (IsClient)
        {
            playerRole.OnValueChanged += OnPlayerRoleChanged;
        }
    }

    // Appel� quand le r�le est s�lectionn� dans l'UI
    public void SelectRole(string role)
    {
        if (!IsOwner) return;

        PlayerRole selectedRole = (PlayerRole)System.Enum.Parse(typeof(PlayerRole), role);
        SetRoleServerRpc(selectedRole);
    }

    [ServerRpc]
    private void SetRoleServerRpc(PlayerRole role)
    {
        playerRole.Value = role;
    }

    private void OnPlayerRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        if (!IsOwner) return;

        // Chargement de la sc�ne appropri�e
        string targetScene = newRole == PlayerRole.Specialiste ?
            specialisteSceneName : technicienSceneName;

        LoadSceneClientRpc(targetScene);
    }

    [ClientRpc]
    private void LoadSceneClientRpc(string sceneName)
    {
        // D�sactiver Vuforia pendant le chargement
        if (vuforiaBehaviour != null)
            vuforiaBehaviour.enabled = false;

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).completed += (op) =>
        {
            // R�activer Vuforia apr�s le chargement
            if (vuforiaBehaviour != null)
                vuforiaBehaviour.enabled = true;
        };
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Configuration additionnelle pour le nouveau client si n�cessaire
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }
}