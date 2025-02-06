using UnityEngine;
using Unity.Netcode;
using HelloWorld;

public class SpecialistObjectInteraction : NetworkBehaviour
{
    [SerializeField] private Material specialistMaterial;
    [SerializeField] private Material technicianMaterial;
    [SerializeField] private GameObject objectToSpawn;
    private Renderer objectRenderer;
    private NetworkVariable<PlayerRole> currentAllowedRole = new NetworkVariable<PlayerRole>(PlayerRole.None);
    private bool isSpawned = false;

    // Ajout de NetworkVariables pour la position et la rotation
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();

    void Start()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        // Vérifier que l'objet est bien dans la liste des prefabs
        if (objectToSpawn != null && NetworkManager.Singleton != null)
        {
            bool prefabFound = false;
            foreach (var prefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)  // Changé PrefabList en Prefabs
            {
                if (prefab.Prefab == objectToSpawn)
                {
                    prefabFound = true;
                    break;
                }
            }
            if (!prefabFound)
            {
                Debug.LogError($"Le prefab {objectToSpawn.name} n'est pas dans la liste des NetworkPrefabs!");
            }
        }
    }

    public void OnTargetFound(Transform targetTransform)
    {
        Debug.Log("Target trouvée ! Tentative de spawn...");

        if (IsServer)
        {
            SpawnObjectOnServer(targetTransform.position, targetTransform.rotation);
        }
        else if (IsClient)
        {
            RequestSpawnServerRpc(targetTransform.position, targetTransform.rotation);
        }
    }

    private void SpawnObjectOnServer(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (isSpawned || objectToSpawn == null)
        {
            Debug.LogWarning("Objet déjà spawné ou prefab manquant.");
            return;
        }

        Debug.Log($"Tentative d'instantiation à la position {targetPosition} et rotation {targetRotation}");

        GameObject spawnedObject = Instantiate(objectToSpawn, targetPosition, targetRotation);
        if (spawnedObject == null)
        {
            Debug.LogError("Erreur lors de l'instanciation de l'objet.");
            return;
        }

        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            // Synchronise la position et la rotation via NetworkVariables
            networkPosition.Value = targetPosition;
            networkRotation.Value = targetRotation;

            networkObject.Spawn();
            spawnedObject.transform.SetParent(null); // Pas besoin de parent pour l'instant
            isSpawned = true;
            Debug.Log("Objet spawné avec succès!");
        }
        else
        {
            Debug.LogError("Le NetworkObject est manquant sur le prefab.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!IsServer) return;
        // Assurez-vous que l'objet n'est pas déjà spawné avant d'effectuer l'opération
        if (!isSpawned)
        {
            SpawnObjectOnServer(targetPosition, targetRotation);
        }
        else
        {
            Debug.LogWarning("Tentative de spawn ignorée car un objet est déjà spawné.");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentAllowedRole.OnValueChanged += OnRoleAllowedChanged;
        UpdateMaterialBasedOnRole();
    }

    private void UpdateMaterialBasedOnRole()
    {
        if (!IsServer) return;
        if (objectRenderer == null) return;
        objectRenderer.material = technicianMaterial;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetInteractionPermissionServerRpc(PlayerRole requestingRole)
    {
        if (requestingRole == PlayerRole.Technicien)
        {
            currentAllowedRole.Value = PlayerRole.Technicien;
            if (objectRenderer != null)
                objectRenderer.material = specialistMaterial;
        }
        else
        {
            currentAllowedRole.Value = PlayerRole.Specialiste;
            if (objectRenderer != null)
                objectRenderer.material = technicianMaterial;
        }
    }

    private void OnRoleAllowedChanged(PlayerRole previousRole, PlayerRole newRole)
    {
        Debug.Log($"Object interaction permission changed from {previousRole} to {newRole}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryInteractServerRpc(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogError("Client ID non trouvé!");
            return;
        }

        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        var player = playerObject.GetComponent<HelloWorldPlayer>();
        if (player == null)
        {
            Debug.LogError("HelloWorldPlayer component not found!");
            return;
        }

        if (player.Role.Value == PlayerRole.Technicien &&
            currentAllowedRole.Value == PlayerRole.Technicien)
        {
            Debug.Log("Technician interacted with the object!");
            transform.position += Vector3.up * 0.5f;
            isSpawned = false;  // Réinitialiser isSpawned après interaction
        }
        else
        {
            Debug.Log("Object cannot be interacted with by this role.");
        }
    }

    void OnMouseDown()
    {
        if (!IsClient) return;
        TryInteractServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    // Fonction qui synchronise la position et la rotation de l'objet pour tous les clients
    [ClientRpc]
    private void UpdateObjectPositionClientRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    // Cette fonction est appelée depuis le serveur pour mettre à jour la position et la rotation
    [ServerRpc(RequireOwnership = false)]
    private void RequestPositionUpdateServerRpc(Vector3 newPosition, Quaternion newRotation)
    {
        // Mettre à jour la position et la rotation
        networkPosition.Value = newPosition;
        networkRotation.Value = newRotation;

        // Envoyer la mise à jour aux clients
        UpdateObjectPositionClientRpc(newPosition, newRotation);
    }

    public override void OnDestroy()
    {
        if (currentAllowedRole != null)
            currentAllowedRole.OnValueChanged -= OnRoleAllowedChanged;

        base.OnDestroy();
    }
}
