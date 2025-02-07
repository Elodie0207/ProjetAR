using Unity.Netcode;
using UnityEngine;
using Vuforia;

public class ARManager : NetworkBehaviour
{
    [SerializeField] private GameObject arCameraPrefab;
    private static ARManager instance;
    private VuforiaBehaviour vuforiaBehaviour;
    private GameObject localARCamera;

    private void Awake()
    {
        // Assure qu'il n'y a qu'une seule instance
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Récupère la référence au VuforiaBehaviour
        vuforiaBehaviour = FindObjectOfType<VuforiaBehaviour>();
        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.enabled = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            InitializeLocalARCamera();
        }
    }

    private void InitializeLocalARCamera()
    {
        // Désactive toutes les caméras AR existantes
        var existingCameras = FindObjectsOfType<Camera>();
        foreach (var cam in existingCameras)
        {
            if (cam.gameObject.CompareTag("ARCamera"))
            {
                cam.gameObject.SetActive(false);
            }
        }

        // Crée une nouvelle caméra AR pour le client local
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        localARCamera = Instantiate(arCameraPrefab, spawnPosition, Quaternion.identity);
        localARCamera.name = $"ARCamera_Client_{NetworkManager.Singleton.LocalClientId}";

        // Configure la caméra AR
        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.enabled = true;
        }

        // Demande au serveur de spawn la caméra en réseau
        SpawnARCameraServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnARCameraServerRpc(ulong clientId)
    {
        // Spawn la caméra AR avec l'ownership pour le client spécifique
        NetworkObject netObj = localARCamera.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnWithOwnership(clientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (localARCamera != null)
        {
            Destroy(localARCamera);
        }
        
        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.enabled = false;
        }
    }
}