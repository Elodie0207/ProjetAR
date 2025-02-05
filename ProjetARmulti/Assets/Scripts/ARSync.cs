using UnityEngine;
using Unity.Netcode;
using Vuforia;

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

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        sharedPosition.Value = position;
        sharedRotation.Value = rotation;
        SyncTransformClientRpc();
    }

    [ClientRpc]
    private void SyncTransformClientRpc()
    {
        if (!IsServer)
        {
            transform.position = sharedPosition.Value;
            transform.rotation = sharedRotation.Value;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
    }
}