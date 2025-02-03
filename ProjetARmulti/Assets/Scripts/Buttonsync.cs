using UnityEngine;
using Unity.Netcode;
using HelloWorld;

public class ButtonSync : NetworkBehaviour
{
    [Header("Boutons")]
    public BoxCollider boutonJoueur1;
    public BoxCollider boutonJoueur2;

    [Header("Configuration")]
    public float delaiMaxEntreAppuis = 1.0f;

    [Header("Feedback")]
    public GameObject objetAActiver;

    // Network variables pour synchroniser l'état entre serveur et clients
    private NetworkVariable<bool> joueur1Appuye = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> joueur2Appuye = new NetworkVariable<bool>(false);
    private NetworkVariable<float> tempsAppuiJoueur1 = new NetworkVariable<float>(0f);
    private NetworkVariable<float> tempsAppuiJoueur2 = new NetworkVariable<float>(0f);

    private void Update()
    {
        // Seulement le client peut interagir
        if (!IsClient) return;

        // Pour les tests PC avec souris
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == boutonJoueur1)
                {
                    OnJoueur1ClickServerRpc();
                }
                else if (hit.collider == boutonJoueur2)
                {
                    OnJoueur2ClickServerRpc();
                }
            }
        }

        // Pour mobile (garder la partie tactile)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == boutonJoueur1)
                {
                    OnJoueur1ClickServerRpc();
                }
                else if (hit.collider == boutonJoueur2)
                {
                    OnJoueur2ClickServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnJoueur1ClickServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Vérifier le rôle du joueur
        var player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(serverRpcParams.Receive.SenderClientId)
            .GetComponent<HelloWorldPlayer>();

        if (player.Role.Value != PlayerRole.Technicien) return;

        joueur1Appuye.Value = true;
        tempsAppuiJoueur1.Value = NetworkManager.Singleton.ServerTime.Time;
        Debug.Log("Joueur 1 a appuyé ! En attente du Joueur 2...");

        VerifierSynchronisationClientRpc();

        // Planifier un reset après un délai
        StartCoroutine(ResetJoueur1Apres(delaiMaxEntreAppuis));
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnJoueur2ClickServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Vérifier le rôle du joueur
        var player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(serverRpcParams.Receive.SenderClientId)
            .GetComponent<HelloWorldPlayer>();

        if (player.Role.Value != PlayerRole.Specialiste) return;

        joueur2Appuye.Value = true;
        tempsAppuiJoueur2.Value = NetworkManager.Singleton.ServerTime.Time;
        Debug.Log("Joueur 2 a appuyé ! En attente du Joueur 1...");

        VerifierSynchronisationClientRpc();

        // Planifier un reset après un délai
        StartCoroutine(ResetJoueur2Apres(delaiMaxEntreAppuis));
    }

    [ClientRpc]
    private void VerifierSynchronisationClientRpc()
    {
        if (!IsServer) return;

        if (joueur1Appuye.Value && joueur2Appuye.Value)
        {
            float diffTemps = Mathf.Abs(tempsAppuiJoueur1.Value - tempsAppuiJoueur2.Value);
            if (diffTemps <= delaiMaxEntreAppuis)
            {
                Debug.Log("SUCCÈS ! Les deux joueurs ont appuyé en même temps !");
                ActionSynchroniseeClientRpc();
            }
            else
            {
                Debug.Log("ÉCHEC ! L'appui n'était pas synchronisé - Différence : " + diffTemps.ToString("F2") + " secondes");
                ResetToutClientRpc();
            }
        }
    }

    [ClientRpc]
    private void ActionSynchroniseeClientRpc()
    {
        if (objetAActiver != null)
        {
            objetAActiver.SetActive(true);
            Debug.Log("Objet activé avec succès !");
        }
        ResetToutClientRpc();
    }

    [ClientRpc]
    private void ResetToutClientRpc()
    {
        joueur1Appuye.Value = false;
        joueur2Appuye.Value = false;
    }

    private System.Collections.IEnumerator ResetJoueur1Apres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (joueur1Appuye.Value)
        {
            joueur1Appuye.Value = false;
            Debug.Log("Temps écoulé pour Joueur 1 - Réessayez !");
        }
    }

    private System.Collections.IEnumerator ResetJoueur2Apres(float delai)
    {
        yield return new WaitForSeconds(delai);
        if (joueur2Appuye.Value)
        {
            joueur2Appuye.Value = false;
            Debug.Log("Temps écoulé pour Joueur 2 - Réessayez !");
        }
    }

    public override void OnNetworkSpawn()
    {
        // Configuration initiale si nécessaire
        base.OnNetworkSpawn();
    }
}