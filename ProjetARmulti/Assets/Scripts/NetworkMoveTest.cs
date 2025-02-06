using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkMoveTest : NetworkBehaviour
{
    [SerializeField] private Button moveButton;

    // Fonction publique qui sera visible dans l'inspecteur
    public void Move()
    {
        Debug.Log("Move called");
        if (IsServer)
        {
            PerformMoveServerRpc();
        }
        else if (IsClient)
        {
            RequestMoveServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMoveServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("RequestMoveServerRpc received");
        PerformMoveServerRpc();
    }

    [ServerRpc]
    private void PerformMoveServerRpc()
    {
        Debug.Log("PerformMoveServerRpc executing");
        // Déplacer vers le haut de 0.5 unités
        transform.position += Vector3.up * 0.5f;

        // Informer tous les clients du mouvement
        MoveClientRpc(transform.position);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector3 newPosition)
    {
        Debug.Log($"MoveClientRpc received. New position: {newPosition}");
        transform.position = newPosition;
    }
}