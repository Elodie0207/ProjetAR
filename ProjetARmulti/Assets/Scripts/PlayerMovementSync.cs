using Unity.Netcode;
using UnityEngine;

public class PlayerMovementSync : NetworkBehaviour
{
    private NetworkVariable<Vector3> syncedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> syncedRotation = new NetworkVariable<Quaternion>();

    void Update()
    {
        if (IsOwner)
        {
            // Envoyer la position et rotation du propri�taire au serveur
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
        else
        {
            // Pour les clients non-propri�taires, synchroniser la position
            transform.position = syncedPosition.Value;
            transform.rotation = syncedRotation.Value;
        }
    }

    [ServerRpc]
    void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        // Le serveur met � jour les variables r�seau
        syncedPosition.Value = position;
        syncedRotation.Value = rotation;
    }
}