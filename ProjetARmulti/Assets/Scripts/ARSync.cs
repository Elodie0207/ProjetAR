using UnityEngine;
using Unity.Netcode;
using Vuforia;
using System.Linq;
using HelloWorld;
using UnityEngine.SceneManagement;
using TMPro;

public class ARSync : NetworkBehaviour
{
    [Header("AR Setup")]
    [SerializeField] private VuforiaBehaviour vuforiaBehaviour;
    [SerializeField] private GameObject testCube;

    // Variables réseau pour la synchronisation
    private NetworkVariable<Vector3> sharedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> sharedRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<bool> isTracking = new NetworkVariable<bool>(false);
    private NetworkVariable<string> currentSceneName = new NetworkVariable<string>();

    private HelloWorldPlayer playerScript;
    private TextMeshPro roleText;

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
        // S'abonner à l'événement de chargement de scène
        SceneManager.sceneLoaded += OnSceneLoaded;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mettre à jour le nom de la scène courante
        if (IsOwner)
        {
            currentSceneName.Value = scene.name;
        }

        // Réinitialiser les références Vuforia si nécessaire
        if (vuforiaBehaviour == null)
        {
            vuforiaBehaviour = FindObjectOfType<VuforiaBehaviour>();
        }

        // Réinitialiser le cube de test si nécessaire
        if (testCube == null)
        {
            testCube = GameObject.FindGameObjectWithTag("TestCube");
        }
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
        if (!IsSpawned) return;

        if (IsOwner && isTracking.Value)
        {
            // Envoyer les mises à jour de position même dans une autre scène
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
        else if (!IsOwner && isTracking.Value)
        {
            // Appliquer les positions partagées
            transform.position = sharedPosition.Value;
            transform.rotation = sharedRotation.Value;
        }

        // Debug logs
        if (IsOwner && isTracking.Value)
        {
            Debug.Log($"Scène actuelle : {currentSceneName.Value}, Position envoyée : {transform.position}");
        }
        else if (!IsOwner && isTracking.Value)
        {
            Debug.Log($"Scène actuelle : {currentSceneName.Value}, Position reçue : {sharedPosition.Value}");
        }

        // Mise à jour du texte de rôle face à la caméra
        if (roleText != null && Camera.main != null)
        {
            roleText.transform.forward = Camera.main.transform.forward;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        // Mettre à jour les variables réseau
        sharedPosition.Value = position;
        sharedRotation.Value = rotation;

        // Synchroniser avec tous les clients
        SyncTransformClientRpc(position, rotation);
    }

    [ClientRpc]
    private void SyncTransformClientRpc(Vector3 position, Quaternion rotation)
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
        // Désabonner des événements
        SceneManager.sceneLoaded -= OnSceneLoaded;

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