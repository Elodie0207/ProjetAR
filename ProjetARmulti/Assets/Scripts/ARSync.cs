using UnityEngine;
using Unity.Netcode;
using Vuforia;
using System.Linq;

public class ARSync : NetworkBehaviour
{
    [Header("AR Setup")]
    [SerializeField] private VuforiaBehaviour vuforiaBehaviour;

    private NetworkVariable<Vector3> sharedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> sharedRotation = new NetworkVariable<Quaternion>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeAR();
        }
    }

    private void InitializeAR()
    {
        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        sharedPosition.Value = position;
        sharedRotation.Value = rotation;

        SyncTransformClientRpc(position, rotation, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != senderId)
                    .ToArray()
            }
        });
    }

    [ClientRpc]
    private void SyncTransformClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }

    void Update()
    {
        if (IsServer || IsOwner)
        {
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}