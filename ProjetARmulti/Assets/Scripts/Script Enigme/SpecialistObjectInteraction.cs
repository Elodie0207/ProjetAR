using UnityEngine;
using Unity.Netcode;
using HelloWorld;

public class SpecialistObjectInteraction : NetworkBehaviour
{
    // Materials for different roles
    [SerializeField] private Material specialistMaterial;
    [SerializeField] private Material technicianMaterial;

    // Reference to the object's renderer
    private Renderer objectRenderer;

    // Network variables to track object state
    private NetworkVariable<PlayerRole> currentAllowedRole = new NetworkVariable<PlayerRole>(PlayerRole.None);

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        // Setup network variable callbacks
        currentAllowedRole.OnValueChanged += OnRoleAllowedChanged;

        // Initial setup of materials
        UpdateMaterialBasedOnRole();
    }

    private void UpdateMaterialBasedOnRole()
    {
        if (!IsServer) return;

        // Only update renderer if we have a renderer component
        if (objectRenderer == null) return;

        // Set default to technician material
        objectRenderer.material = technicianMaterial;
    }

    // Server-side method to set object interaction permission
    [ServerRpc(RequireOwnership = false)]
    public void SetInteractionPermissionServerRpc(PlayerRole requestingRole)
    {
        // Invert the material logic
        if (requestingRole == PlayerRole.Technicien)
        {
            currentAllowedRole.Value = PlayerRole.Technicien;
            objectRenderer.material = specialistMaterial;
        }
        else
        {
            currentAllowedRole.Value = PlayerRole.Specialiste;
            objectRenderer.material = technicianMaterial;
        }
    }

    // Callback when allowed role changes
    private void OnRoleAllowedChanged(PlayerRole previousRole, PlayerRole newRole)
    {
        Debug.Log($"Object interaction permission changed from {previousRole} to {newRole}");
    }

    // Client-side interaction method
    [ServerRpc(RequireOwnership = false)]
    private void TryInteractServerRpc(ulong clientId)
    {
        // Find the player object
        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObject == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        // Get the player's role
        var player = playerObject.GetComponent<HelloWorldPlayer>();
        if (player == null)
        {
            Debug.LogError("HelloWorldPlayer component not found!");
            return;
        }

        // Check if the player can interact
        // Inverting the interaction logic
        if (player.Role.Value == PlayerRole.Technicien &&
            currentAllowedRole.Value == PlayerRole.Technicien)
        {
            // Perform interaction logic here
            Debug.Log("Technician interacted with the object!");

            // Example of potential interaction - you can expand this
            transform.position += Vector3.up * 0.5f; // Simple vertical movement
        }
        else
        {
            Debug.Log("Object cannot be interacted with by this role.");
        }
    }

    void OnMouseDown()
    {
        // Only attempt interaction if this is a client
        if (!IsClient) return;

        // Attempt to interact using the local client ID
        TryInteractServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public override void OnDestroy()
    {
        // Cleanup network variable callbacks
        if (currentAllowedRole != null)
            currentAllowedRole.OnValueChanged -= OnRoleAllowedChanged;

        base.OnDestroy();
    }
}