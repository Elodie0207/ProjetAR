using UnityEngine;
using Unity.Netcode;
using HelloWorld;

public class ARButtonSync : NetworkBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private float delaiMaxEntreAppuis = 1.0f;

    private NetworkVariable<bool> technicienAppuye = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> specialisteAppuye = new NetworkVariable<bool>(false);
    private NetworkVariable<float> tempsAppuiTechnicien = new NetworkVariable<float>(0f);
    private NetworkVariable<float> tempsAppuiSpecialiste = new NetworkVariable<float>(0f);
    private bool buttonSpawned = false;
    private GameObject spawnedButton;

    void Start()
    {
        Debug.Log($"Start - ButtonPrefab assign�: {buttonPrefab != null}, IsServer: {IsServer}");
    }

    public void OnTargetFound()
    {
        Debug.Log($"Target trouv�e! IsServer: {IsServer}, buttonSpawned: {buttonSpawned}, buttonPrefab: {buttonPrefab != null}");

        if (!IsServer)
        {
            Debug.Log("Client d�tect�, demande au serveur");
            SpawnRequestServerRpc();
            return;
        }

        SpawnButton();
    }

    private void SpawnButton()
    {
        Debug.Log("SpawnButton appel�");
        if (buttonSpawned)
        {
            Debug.Log("Bouton d�j� spawn�");
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("ButtonPrefab non assign�!");
            return;
        }

        Debug.Log("Tentative d'instantiation du bouton");
        spawnedButton = Instantiate(buttonPrefab, transform.position, transform.rotation);

        if (spawnedButton != null)
        {
            NetworkObject netObj = spawnedButton.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
                spawnedButton.transform.SetParent(transform);
                spawnedButton.transform.localPosition = Vector3.zero;
                buttonSpawned = true;
                Debug.Log("Bouton spawn� avec succ�s!");
            }
            else
            {
                Debug.LogError("Pas de NetworkObject sur le bouton!");
                Destroy(spawnedButton);
            }
        }
        else
        {
            Debug.LogError("�chec de l'instantiation!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnRequestServerRpc()
    {
        Debug.Log("SpawnRequestServerRpc re�u");
        SpawnButton();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clic gauche
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (!IsClient) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == spawnedButton)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                if (playerObject == null) return;

                var player = playerObject.GetComponent<HelloWorldPlayer>();
                if (player == null) return;

                if (player.Role.Value == PlayerRole.Technicien)
                {
                    OnTechnicienClickServerRpc();
                }
                else if (player.Role.Value == PlayerRole.Specialiste)
                {
                    OnSpecialisteClickServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnTechnicienClickServerRpc()
    {
        technicienAppuye.Value = true;
        tempsAppuiTechnicien.Value = Time.time;
        Debug.Log("Technicien a appuy�!");
        VerifierSynchronisation();
        StartCoroutine(ResetTechnicienApres(delaiMaxEntreAppuis));
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnSpecialisteClickServerRpc()
    {
        specialisteAppuye.Value = true;
        tempsAppuiSpecialiste.Value = Time.time;
        Debug.Log("Sp�cialiste a appuy�!");
        VerifierSynchronisation();
        StartCoroutine(ResetSpecialisteApres(delaiMaxEntreAppuis));
    }

    private void VerifierSynchronisation()
    {
        if (!IsServer) return;

        if (technicienAppuye.Value && specialisteAppuye.Value)
        {
            float diffTemps = Mathf.Abs(tempsAppuiTechnicien.Value - tempsAppuiSpecialiste.Value);
            if (diffTemps <= delaiMaxEntreAppuis)
            {
                Debug.Log("Synchronisation r�ussie!");
                SynchronisationReussieClientRpc();
            }
            ResetBoutonsClientRpc();
        }
    }

    [ClientRpc]
    private void SynchronisationReussieClientRpc()
    {
        Debug.Log("Les deux joueurs ont appuy� en m�me temps!");
        // Ajoutez ici ce qui doit se passer quand les joueurs r�ussissent
    }

    [ClientRpc]
    private void ResetBoutonsClientRpc()
    {
        technicienAppuye.Value = false;
        specialisteAppuye.Value = false;
    }

    private System.Collections.IEnumerator ResetTechnicienApres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (technicienAppuye.Value)
        {
            technicienAppuye.Value = false;
            Debug.Log("Temps �coul� pour le Technicien!");
        }
    }

    private System.Collections.IEnumerator ResetSpecialisteApres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (specialisteAppuye.Value)
        {
            specialisteAppuye.Value = false;
            Debug.Log("Temps �coul� pour le Sp�cialiste!");
        }
    }
}