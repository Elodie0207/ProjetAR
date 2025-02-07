using HelloWorld;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Buttonsynctest : NetworkBehaviour
{
    [SerializeField] private GameObject buttonTarget; // Référence au bouton 3D dans la scène
    [SerializeField] private float delaiMaxEntreAppuis = 1.0f;

    private NetworkVariable<bool> technicienAppuye = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> specialisteAppuye = new NetworkVariable<bool>(false);
    private NetworkVariable<float> tempsAppuiTechnicien = new NetworkVariable<float>(0f);
    private NetworkVariable<float> tempsAppuiSpecialiste = new NetworkVariable<float>(0f);

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clic gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == buttonTarget)
                {
                    OnButtonClicked();
                }
            }
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log($"Bouton cliqué ! IsServer: {IsServer}, IsOwner: {IsOwner}");

        if (IsOwner)
        {
            if (NetworkManager.Singleton.LocalClientId == 0)
            {
                Debug.Log("Envoi du clic Technicien");
                OnTechnicienClickServerRpc();
            }
            else if (NetworkManager.Singleton.LocalClientId == 1)
            {
                Debug.Log("Envoi du clic Spécialiste");
                OnSpecialisteClickServerRpc();
            }
        }

        if (!IsServer)
        {
            Debug.Log("Client détecté, demande au serveur");
            OnButtonClickedServerRpc();
            return;
        }

        // Logique côté serveur pour gérer le clic sur le bouton
        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObject == null)
        {
            Debug.LogError("Player object non trouvé");
            return;
        }

        var player = playerObject.GetComponent<HelloWorldPlayer>();
        if (player == null)
        {
            Debug.LogError("HelloWorldPlayer component non trouvé");
            return;
        }

        Debug.Log($"Rôle du joueur: {player.Role.Value}");

        if (player.Role.Value == PlayerRole.Technicien)
        {
            Debug.Log("Envoi du clic Technicien");
            OnTechnicienClickServerRpc();
        }
        else if (player.Role.Value == PlayerRole.Specialiste)
        {
            Debug.Log("Envoi du clic Spécialiste");
            OnSpecialisteClickServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnButtonClickedServerRpc()
    {
        // Logique côté serveur pour gérer le clic sur le bouton
        OnButtonClicked();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnTechnicienClickServerRpc()
    {
        Debug.Log("OnTechnicienClickServerRpc reçu!");
        technicienAppuye.Value = true;
        tempsAppuiTechnicien.Value = Time.time;
        Debug.Log("Technicien a appuyé!");
        VerifierSynchronisation();
        StartCoroutine(ResetTechnicienApres(delaiMaxEntreAppuis));
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnSpecialisteClickServerRpc()
    {
        Debug.Log("OnSpecialisteClickServerRpc reçu!");
        specialisteAppuye.Value = true;
        tempsAppuiSpecialiste.Value = Time.time;
        Debug.Log("Spécialiste a appuyé!");
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
                Debug.Log("Synchronisation réussie!");
                SynchronisationReussieClientRpc();
            }
            ResetBoutonsClientRpc();
        }
    }

    [ClientRpc]
    private void SynchronisationReussieClientRpc()
    {
        Debug.Log("Les deux joueurs ont appuyé en même temps!");
        // Ajoutez ici ce qui doit se passer quand les joueurs réussissent
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
            Debug.Log("Temps écoulé pour le Technicien!");
        }
    }

    private IEnumerator ResetSpecialisteApres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (specialisteAppuye.Value)
        {
            specialisteAppuye.Value = false;
            Debug.Log("Temps écoulé pour le Spécialiste!");
        }
    }
}