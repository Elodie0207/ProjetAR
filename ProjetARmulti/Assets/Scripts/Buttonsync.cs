using UnityEngine;
using Unity.Netcode;
using HelloWorld;

using HelloWorld;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Vuforia;

public class ButtonSyncAR : NetworkBehaviour
{
    [SerializeField] private float delaiMaxEntreAppuis = 1.0f;
    [SerializeField] private GameObject buttonPrefab;

    private NetworkVariable<bool> technicienAppuye = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> specialisteAppuye = new NetworkVariable<bool>(false);
    private NetworkVariable<float> tempsAppuiTechnicien = new NetworkVariable<float>(0f);
    private NetworkVariable<float> tempsAppuiSpecialiste = new NetworkVariable<float>(0f);
    private bool buttonSpawned = false;
    private GameObject spawnedButton;

    private void OnTrackingFound()
    {
        if (!buttonSpawned)
        {
            SpawnButton();
        }
    }

    private void OnTrackingLost()
    {
        if (buttonSpawned && spawnedButton != null)
        {
            Destroy(spawnedButton);
            buttonSpawned = false;
        }
    }

    private void SpawnButton()
    {
        if (!IsServer)
        {
            SpawnButtonServerRpc();
            return;
        }

        spawnedButton = Instantiate(buttonPrefab, transform.position, transform.rotation);
        spawnedButton.GetComponent<NetworkObject>().Spawn();
        buttonSpawned = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnButtonServerRpc()
    {
        SpawnButton();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clic gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == spawnedButton)
                {
                    OnButtonClicked();
                }
            }
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log($"Bouton cliqu� ! IsServer: {IsServer}, IsOwner: {IsOwner}");

        if (IsOwner)
        {
            if (NetworkManager.Singleton.LocalClientId == 0)
            {
                Debug.Log("Envoi du clic Technicien");
                OnTechnicienClickServerRpc();
            }
            else if (NetworkManager.Singleton.LocalClientId == 1)
            {
                Debug.Log("Envoi du clic Sp�cialiste");
                OnSpecialisteClickServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnTechnicienClickServerRpc()
    {
        Debug.Log("OnTechnicienClickServerRpc re�u!");
        technicienAppuye.Value = true;
        tempsAppuiTechnicien.Value = Time.time;
        Debug.Log("Technicien a appuy�!");
        VerifierSynchronisation();
        StartCoroutine(ResetTechnicienApres(delaiMaxEntreAppuis));
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnSpecialisteClickServerRpc()
    {
        Debug.Log("OnSpecialisteClickServerRpc re�u!");
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

    private IEnumerator ResetTechnicienApres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (technicienAppuye.Value)
        {
            technicienAppuye.Value = false;
            Debug.Log("Temps �coul� pour le Technicien!");
        }
    }

    private IEnumerator ResetSpecialisteApres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (specialisteAppuye.Value)
        {
            specialisteAppuye.Value = false;
            Debug.Log("Temps �coul� pour le Sp�cialiste!");
        }
    }
}


/*public class ARButtonSync : NetworkBehaviour
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

        // Obtenir la position et la rotation de la cible
        Vector3 targetPosition = transform.position;
        Quaternion targetRotation = transform.rotation;

        // Ajuster la position pour centrer le bouton sur la cible
        Vector3 buttonPosition = targetPosition + targetRotation * Vector3.forward * 0.1f;

        spawnedButton = Instantiate(buttonPrefab, buttonPosition, targetRotation);

        if (spawnedButton != null)
        {
            NetworkObject netObj = spawnedButton.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
                spawnedButton.transform.SetParent(transform);
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
        if (!IsClient || !buttonSpawned)
        {
            Debug.Log("HandleClick - Pas un client ou bouton non spawn�");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.Log("Tentative de raycast...");

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Hit d�tect� sur: {hit.collider.gameObject.name}");
            // V�rifie si l'objet touch� ou son parent est le bouton
            GameObject hitObject = hit.collider.gameObject;
            bool isButton = hitObject == spawnedButton ||
                           (hitObject.transform.parent != null && hitObject.transform.parent.gameObject == spawnedButton);

            if (isButton)
            {
                Debug.Log("Clic sur le bouton d�tect�!");
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                if (playerObject == null)
                {
                    Debug.LogError("Player object non trouv�");
                    return;
                }

                var player = playerObject.GetComponent<HelloWorldPlayer>();
                if (player == null)
                {
                    Debug.LogError("HelloWorldPlayer component non trouv�");
                    return;
                }

                Debug.Log($"R�le du joueur: {player.Role.Value}");

                if (player.Role.Value == PlayerRole.Technicien)
                {
                    Debug.Log("Envoi du clic Technicien");
                    OnTechnicienClickServerRpc();
                }
                else if (player.Role.Value == PlayerRole.Specialiste)
                {
                    Debug.Log("Envoi du clic Sp�cialiste");
                    OnSpecialisteClickServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnTechnicienClickServerRpc()
    {
        Debug.Log("OnTechnicienClickServerRpc re�u!");
        technicienAppuye.Value = true;
        tempsAppuiTechnicien.Value = Time.time;
        Debug.Log("Technicien a appuy�!");
        VerifierSynchronisation();
        StartCoroutine(ResetTechnicienApres(delaiMaxEntreAppuis));
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnSpecialisteClickServerRpc()
    {
        Debug.Log("OnSpecialisteClickServerRpc re�u!");
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
}*/