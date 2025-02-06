using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkSyncTest : NetworkBehaviour
{
    [SerializeField] private Button colorButton;
    [SerializeField] private GameObject testCube;

    private NetworkVariable<Color> sharedColor = new NetworkVariable<Color>(Color.white);
    private Color[] colors = new Color[] {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta
    };
    private int currentColorIndex = 0;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"NetworkSyncTest spawned. IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");

        // Créer le cube de test s'il n'existe pas
        if (testCube == null)
        {
            testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            testCube.transform.position = new Vector3(0, 0.5f, 0);
            Debug.Log("Test cube created");
        }

        // Configurer le bouton
        if (colorButton != null)
        {
            Debug.Log("Button found, adding listener");
            colorButton.onClick.AddListener(() => {
                Debug.Log("Button clicked!");
                OnColorButtonClicked();
            });
        }
        else
        {
            Debug.LogError("Color Button not assigned in inspector!");
        }
    }

    void Update()
    {
        if (testCube != null)
        {
            var renderer = testCube.GetComponent<Renderer>();
            if (renderer != null && renderer.material.color != sharedColor.Value)
            {
                renderer.material.color = sharedColor.Value;
                Debug.Log($"Updated cube color to: {sharedColor.Value}");
            }
        }
    }

    private void OnColorButtonClicked()
    {
        Debug.Log($"OnColorButtonClicked called. IsServer: {IsServer}, IsClient: {IsClient}");
        if (IsServer)
        {
            Debug.Log("Processing color change on server");
            ChangeColorServerRpc();
        }
        else if (IsClient)
        {
            Debug.Log("Requesting color change from client");
            RequestColorChangeServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestColorChangeServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("RequestColorChangeServerRpc received");
        ChangeColorServerRpc();
    }

    [ServerRpc]
    private void ChangeColorServerRpc()
    {
        Debug.Log("ChangeColorServerRpc executing");
        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        sharedColor.Value = colors[currentColorIndex];
        Debug.Log($"Color changed to: {colors[currentColorIndex]}");
    }

    void OnDestroy()
    {
        if (colorButton != null)
        {
            colorButton.onClick.RemoveAllListeners();
        }
    }
}