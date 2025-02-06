using UnityEngine;
using Unity.Netcode;
using Vuforia;
using System.Linq;
using HelloWorld;

public class ARSync : NetworkBehaviour
{
    [Header("AR Setup")]
    [SerializeField] private VuforiaBehaviour vuforiaBehaviour;
    [SerializeField] private GameObject testCube;

    // Variables réseau pour la synchronisation
    private NetworkVariable<Vector3> sharedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> sharedRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<bool> isTracking = new NetworkVariable<bool>(false);

    private HelloWorldPlayer playerScript;

    void Start()
    {
        if (testCube != null)
        {
            // Couleur différente selon le rôle
            var renderer = testCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = IsOwner ? Color.blue : Color.red;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeAR();
        }

        // Obtenir la référence au script du joueur
        playerScript = GetComponent<HelloWorldPlayer>();

        if (playerScript != null)
        {
            // S'abonner aux changements de rôle
            playerScript.Role.OnValueChanged += OnPlayerRoleChanged;
        }

        // S'assurer que l'objet persiste entre les scènes
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeAR()
    {
        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.enabled = true;
        }
    }

    private void OnPlayerRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        // Mettre à jour le tracking en fonction du rôle
        if (IsOwner)
        {
            isTracking.Value = (newRole != PlayerRole.None);
        }
    }

    void Update()
    {
        if (IsOwner && isTracking.Value)
        {
            // Envoyer les mises à jour de position seulement si on est en train de tracker
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
        else if (!IsOwner && isTracking.Value)
        {
            // Appliquer les positions partagées pour les autres clients
            transform.position = sharedPosition.Value;
            transform.rotation = sharedRotation.Value;
        }

        if (IsOwner && isTracking.Value)
        {
            Debug.Log($"Position envoyée : {transform.position}");
        }
        else if (!IsOwner && isTracking.Value)
        {
            Debug.Log($"Position reçue : {sharedPosition.Value}");
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        sharedPosition.Value = position;
        sharedRotation.Value = rotation;

        // Envoyer la mise à jour à tous les autres clients sauf l'expéditeur
        SyncTransformClientRpc(position, rotation, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != OwnerClientId)
                    .ToArray()
            }
        });
    }

    [ClientRpc]
    private void SyncTransformClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner && isTracking.Value)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }

    public void EnableTracking(bool enable)
    {
        if (IsOwner)
        {
            isTracking.Value = enable;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (playerScript != null)
        {
            playerScript.Role.OnValueChanged -= OnPlayerRoleChanged;
        }

        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.enabled = false;
        }

        base.OnNetworkDespawn();
    }
}