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

    // Variables r�seau pour la synchronisation
    private NetworkVariable<Vector3> sharedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> sharedRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<bool> isTracking = new NetworkVariable<bool>(false);

    private HelloWorldPlayer playerScript;

    void Start()
    {
        if (testCube != null)
        {
            // Couleur diff�rente selon le r�le
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

        // Obtenir la r�f�rence au script du joueur
        playerScript = GetComponent<HelloWorldPlayer>();

        if (playerScript != null)
        {
            // S'abonner aux changements de r�le
            playerScript.Role.OnValueChanged += OnPlayerRoleChanged;
        }

        // S'assurer que l'objet persiste entre les sc�nes
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
        // Mettre � jour le tracking en fonction du r�le
        if (IsOwner)
        {
            isTracking.Value = (newRole != PlayerRole.None);
        }
    }

    void Update()
    {
        if (IsOwner && isTracking.Value)
        {
            // Envoyer les mises � jour de position seulement si on est en train de tracker
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
        else if (!IsOwner && isTracking.Value)
        {
            // Appliquer les positions partag�es pour les autres clients
            transform.position = sharedPosition.Value;
            transform.rotation = sharedRotation.Value;
        }

        if (IsOwner && isTracking.Value)
        {
            Debug.Log($"Position envoy�e : {transform.position}");
        }
        else if (!IsOwner && isTracking.Value)
        {
            Debug.Log($"Position re�ue : {sharedPosition.Value}");
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        sharedPosition.Value = position;
        sharedRotation.Value = rotation;

        // Envoyer la mise � jour � tous les autres clients sauf l'exp�diteur
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