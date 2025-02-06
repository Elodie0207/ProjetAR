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

    // Variables r�seau pour la synchronisation
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
        // S'abonner � l'�v�nement de chargement de sc�ne
        SceneManager.sceneLoaded += OnSceneLoaded;

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
        // Mettre � jour le nom de la sc�ne courante
        if (IsOwner)
        {
            currentSceneName.Value = scene.name;
        }

        // R�initialiser les r�f�rences Vuforia si n�cessaire
        if (vuforiaBehaviour == null)
        {
            vuforiaBehaviour = FindObjectOfType<VuforiaBehaviour>();
        }

        // R�initialiser le cube de test si n�cessaire
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
        // Mettre � jour le tracking en fonction du r�le
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
            // Envoyer les mises � jour de position m�me dans une autre sc�ne
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
        else if (!IsOwner && isTracking.Value)
        {
            // Appliquer les positions partag�es
            transform.position = sharedPosition.Value;
            transform.rotation = sharedRotation.Value;
        }

        // Debug logs
        if (IsOwner && isTracking.Value)
        {
            Debug.Log($"Sc�ne actuelle : {currentSceneName.Value}, Position envoy�e : {transform.position}");
        }
        else if (!IsOwner && isTracking.Value)
        {
            Debug.Log($"Sc�ne actuelle : {currentSceneName.Value}, Position re�ue : {sharedPosition.Value}");
        }

        // Mise � jour du texte de r�le face � la cam�ra
        if (roleText != null && Camera.main != null)
        {
            roleText.transform.forward = Camera.main.transform.forward;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        // Mettre � jour les variables r�seau
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
        // D�sabonner des �v�nements
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