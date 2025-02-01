using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        private static NetworkManager m_NetworkManager;
        private bool showRoleSelection = false;

        void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();
            Time.timeScale = 0; // Stopper le temps tant qu'on n'est pas connecté
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                Time.timeScale = 1; // Reprendre le temps quand connecté
                StatusLabels();

                // Afficher la sélection de rôle si connecté et rôle non sélectionné
                var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                if (playerObject != null)
                {
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    if (player.Role.Value == PlayerRole.None)
                    {
                        DrawRoleSelection();
                    }
                    else
                    {
                        SubmitNewPosition();
                    }
                }
            }

            GUILayout.EndArea();
        }

        void DrawRoleSelection()
        {
            GUILayout.Label("Choisissez votre rôle :");
            
            if (GUILayout.Button("Specialiste"))
            {
                SelectRole(PlayerRole.Specialiste);
            }
            
            if (GUILayout.Button("Technicien"))
            {
                SelectRole(PlayerRole.Technicien);
            }
        }

        void SelectRole(PlayerRole role)
        {
            var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
            if (playerObject != null)
            {
                var player = playerObject.GetComponent<HelloWorldPlayer>();
                player.SetRoleServerRpc(role);
            }
        }

        static void StartButtons()
        {
            if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
            if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
            if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
        }

        static void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(m_NetworkManager.IsServer ? "Move" : "Request Position Change"))
            {
                if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
                {
                    foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                        m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
                }
                else
                {
                    var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    Debug.Log("client is: " + playerObject.name);
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
        }
    }

    public class PlayerController : MonoBehaviour
    {
        void Start()
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                enabled = false; // Désactiver les contrôles tant qu'on n'est pas connecté
            }
        }

        void Update()
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                enabled = true; // Activer les contrôles après connexion
            }
        }
    }
}
