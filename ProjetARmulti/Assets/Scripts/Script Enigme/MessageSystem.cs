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

        // S'assurer que le joueur est bien connecté avant d'exécuter le code
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObject != null)
        {
            var player = playerObject.GetComponent<HelloWorldPlayer>();
            if (player != null && player.Role.Value == PlayerRole.Technicien)
            {
                HideDrawingPlane();
            }
        }
    }

    private void HideDrawingPlane()
    {
        GameObject plane = GameObject.FindWithTag("DrawingPlane"); // Assurez-vous que le plane a ce tag
        if (plane != null)
        {
            plane.GetComponent<MeshRenderer>().enabled = false; // Cache seulement l'affichage
            // plane.SetActive(false); // Désactive complètement si nécessaire
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
                StartDrawingServerRpc(hitPoint.Value);
                isDrawing = true;
            }
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3? hitPoint = GetMouseHitPoint();
            if (hitPoint.HasValue)
            {
                ContinueDrawingServerRpc(hitPoint.Value);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            EndDrawingServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearDrawingsServerRpc();
        }
    }

    private Vector3? GetMouseHitPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, drawingPlane))
        {
            return hit.point;
        }

        // Si aucun plane détecté (cas du technicien), projeter sur un plan horizontal
        Plane virtualPlane = new Plane(Vector3.up, Vector3.zero); // Plan horizontal à Y=0
        if (virtualPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartDrawingServerRpc(Vector3 startPoint)
    {
        GameObject lineObj = Instantiate(lineRendererPrefab);
        NetworkObject netObj = lineObj.GetComponent<NetworkObject>();
        netObj.Spawn();
        ConfigureLineRendererClientRpc(netObj.NetworkObjectId, startPoint);
    }

    [ClientRpc]
    private void ConfigureLineRendererClientRpc(ulong networkObjectId, Vector3 startPoint)
    {
        GameObject lineObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;
        currentLine = lineObj.GetComponent<LineRenderer>();
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.material.color = drawColor;
        currentLinePositions.Clear();
        AddPointToLineClientRpc(startPoint);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ContinueDrawingServerRpc(Vector3 newPoint)
    {
        AddPointToLineClientRpc(newPoint);
    }

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

    [ServerRpc(RequireOwnership = false)]
    private void EndDrawingServerRpc()
    {
        EndDrawingClientRpc();
    }

    [ClientRpc]
    private void EndDrawingClientRpc()
    {
        currentLine = null;
        currentLinePositions.Clear();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearDrawingsServerRpc()
    {
        ClearDrawingsClientRpc();
    }

    [ClientRpc]
    private void ClearDrawingsClientRpc()
    {
        LineRenderer[] lines = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer line in lines)
        {
            Destroy(line.gameObject);
        }
        currentLine = null;
        currentLinePositions.Clear();
    }
}
