using HelloWorld;
using UnityEngine;
using Unity.Netcode;

public class SpecialistObjectInteraction : NetworkBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Material specialistMaterial;
    [SerializeField] private Material technicianMaterial;

    private NetworkVariable<Quaternion> syncedRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private GameObject spawnedObject;
    private NetworkObject spawnedNetObject;
    private NetworkVariable<bool> hasSpawned = new NetworkVariable<bool>(false);
    private NetworkVariable<ulong> objectOwner = new NetworkVariable<ulong>();
    private NetworkVariable<NetworkObjectReference> spawnedObjectRef = new NetworkVariable<NetworkObjectReference>();
    private NetworkVariable<bool> isSpecialist = new NetworkVariable<bool>(true);

    private bool isObjectSelected = false;  // Indique si l'objet est sélectionné pour rotation
    private float rotationSpeed = 30f;  // Ajuste cette valeur pour la vitesse de rotation

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spawnedObjectRef.OnValueChanged += OnSpawnedObjectChanged;
        syncedRotation.OnValueChanged += OnSyncedRotationChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        spawnedObjectRef.OnValueChanged -= OnSpawnedObjectChanged;
        syncedRotation.OnValueChanged -= OnSyncedRotationChanged;
    }

    private void OnSyncedRotationChanged(Quaternion previousValue, Quaternion newValue)
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.rotation = newValue;
        }
    }

    private void OnSpawnedObjectChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        if (newValue.TryGet(out NetworkObject netObj))
        {
            spawnedObject = netObj.gameObject;
            UpdateMaterialClientRpc(isSpecialist.Value);
        }
    }

    [ClientRpc]
    private void UpdateMaterialClientRpc(bool isSpecialistRole)
    {
        if (spawnedObject != null)
        {
            Renderer renderer = spawnedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (NetworkManager.Singleton.LocalClientId == objectOwner.Value)
                {
                    renderer.material = specialistMaterial;
                }
                else
                {
                    renderer.material = technicianMaterial;
                }
            }
        }
    }

    void Update()
    {
        if (spawnedObject == null || !hasSpawned.Value) return;

        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObject == null) return;

        var player = playerObject.GetComponent<HelloWorldPlayer>();
        if (player == null || player.Role.Value != PlayerRole.Technicien) return; // Seul le technicien peut faire tourner l'objet

        // Détection du clic de souris pour sélectionner l'objet
        if (Input.GetMouseButtonDown(0))  // Clic gauche de la souris
        {
            if (IsObjectClicked())
            {
                isObjectSelected = true;
            }
        }

        // Si l'objet est sélectionné et le bouton de la souris est maintenu enfoncé
        if (isObjectSelected && Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (mouseX != 0f || mouseY != 0f)
            {
                // Rotation horizontale autour de l'axe Y (gauche/droite)
                Quaternion horizontalRotation = Quaternion.Euler(0f, mouseX * rotationSpeed, 0f);

                // Rotation verticale autour de l'axe X (haut/bas)
                Quaternion verticalRotation = Quaternion.Euler(-mouseY * rotationSpeed, 0f, 0f);

                // Applique les deux rotations à la transformation de l'objet
                Quaternion newRotation = spawnedObject.transform.rotation * horizontalRotation * verticalRotation;

                // Envoie la rotation au serveur pour la synchroniser avec tous les clients
                UpdateRotationServerRpc(newRotation);
            }
        }

        // Si on relâche le clic, la rotation cesse
        if (Input.GetMouseButtonUp(0))  // Clic gauche de la souris
        {
            isObjectSelected = false;
        }
    }

    // Vérifie si la souris clique sur l'objet
    private bool IsObjectClicked()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject == spawnedObject;
        }

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRotationServerRpc(Quaternion newRotation)
    {
        // Si la rotation a changé, on la met à jour côté serveur
        if (syncedRotation.Value != newRotation)
        {
            syncedRotation.Value = newRotation;  // Mise à jour de la rotation côté serveur
        }
    }

    void OnMouseDown()
    {
        if (!IsClient) return;
        RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(ulong clientId)
    {
        if (hasSpawned.Value) return;

        Vector3 spawnPosition = transform.position + Vector3.up * 1.0f;
        spawnedObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        spawnedNetObject = spawnedObject.GetComponent<NetworkObject>();

        if (spawnedNetObject != null)
        {
            spawnedNetObject.SpawnWithOwnership(clientId);
            objectOwner.Value = clientId;
            hasSpawned.Value = true;
            spawnedObjectRef.Value = new NetworkObjectReference(spawnedNetObject);
            isSpecialist.Value = true;
        }
    }
}
