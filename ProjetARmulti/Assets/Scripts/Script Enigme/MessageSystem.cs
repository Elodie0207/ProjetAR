using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using HelloWorld;

public class MessageSystem : NetworkBehaviour
{
    [SerializeField] private GameObject lineRendererPrefab;
    [SerializeField] private Color drawColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private LayerMask drawingPlane;

    private LineRenderer currentLine;
    private List<Vector3> currentLinePositions = new List<Vector3>();
    private Camera mainCamera;
    private bool isDrawing = false;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
            Debug.LogWarning("Main camera not found, using first available camera");
        }
    }

    private void Update()
    {
        if (!IsClient) return;

        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObject == null) return;

        var player = playerObject.GetComponent<HelloWorldPlayer>();
        if (player == null || player.Role.Value != PlayerRole.Specialiste) return;

        HandleDrawingInput();
    }

    private void HandleDrawingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3? hitPoint = GetMouseHitPoint();
            if (hitPoint.HasValue)
            {
                StartDrawingServerRpc(hitPoint.Value);  // Appel ServerRpc pour créer la ligne
                isDrawing = true;
            }
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3? hitPoint = GetMouseHitPoint();
            if (hitPoint.HasValue)
            {
                ContinueDrawingServerRpc(hitPoint.Value);  // Appel ServerRpc pour continuer la ligne
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            EndDrawingServerRpc();  // Appel ServerRpc pour terminer le dessin
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearDrawingsServerRpc();  // Appel ServerRpc pour effacer les dessins
        }
    }

    private Vector3? GetMouseHitPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, drawingPlane))
        {
            return hit.point;
        }
        return null;
    }

    // ServerRpc pour démarrer le dessin (instancie et synchronise le dessin pour tous les clients)
    [ServerRpc(RequireOwnership = false)]
    private void StartDrawingServerRpc(Vector3 startPoint)
    {
        GameObject lineObj = Instantiate(lineRendererPrefab);
        NetworkObject netObj = lineObj.GetComponent<NetworkObject>();
        netObj.Spawn();  // Spawner l'objet en réseau

        ConfigureLineRendererClientRpc(netObj.NetworkObjectId, startPoint);  // Synchroniser sur les clients
    }

    // ClientRpc pour configurer le LineRenderer
    [ClientRpc]
    private void ConfigureLineRendererClientRpc(ulong networkObjectId, Vector3 startPoint)
    {
        GameObject lineObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;
        currentLine = lineObj.GetComponent<LineRenderer>();
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.material.color = drawColor;
        currentLinePositions.Clear();

        AddPointToLineClientRpc(startPoint);  // Ajouter le premier point au dessin
    }

    // ServerRpc pour continuer à dessiner la ligne
    [ServerRpc(RequireOwnership = false)]
    private void ContinueDrawingServerRpc(Vector3 newPoint)
    {
        AddPointToLineClientRpc(newPoint);  // Ajouter un point au dessin
    }

    // ClientRpc pour ajouter un point au LineRenderer
    [ClientRpc]
    private void AddPointToLineClientRpc(Vector3 point)
    {
        if (currentLine != null)
        {
            currentLinePositions.Add(point);
            currentLine.positionCount = currentLinePositions.Count;
            currentLine.SetPositions(currentLinePositions.ToArray());
        }
    }

    // ServerRpc pour terminer le dessin
    [ServerRpc(RequireOwnership = false)]
    private void EndDrawingServerRpc()
    {
        EndDrawingClientRpc();  // Fin du dessin
    }

    // ClientRpc pour terminer le dessin
    [ClientRpc]
    private void EndDrawingClientRpc()
    {
        currentLine = null;
        currentLinePositions.Clear();
    }

    // ServerRpc pour effacer les dessins
    [ServerRpc(RequireOwnership = false)]
    private void ClearDrawingsServerRpc()
    {
        ClearDrawingsClientRpc();  // Effacer les dessins sur tous les clients
    }

    // ClientRpc pour effacer les dessins
    [ClientRpc]
    private void ClearDrawingsClientRpc()
    {
        LineRenderer[] lines = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer line in lines)
        {
            Destroy(line.gameObject);  // Détruire les objets de dessin localement
        }

        currentLine = null;
        currentLinePositions.Clear();
    }
}
