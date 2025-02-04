using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace HelloWorld
{
    public enum PlayerRole
    {
        None,
        Specialiste,
        Technicien
    }

    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(PlayerRole.None);
        private TextMeshPro roleText;
        
        // Static NetworkVariables pour suivre les rôles sélectionnés
        private static NetworkVariable<bool> _specialisteSelected = new NetworkVariable<bool>();
        private static NetworkVariable<bool> _technicienSelected = new NetworkVariable<bool>();

        public override void OnNetworkSpawn()
        {
            if (IsClient && IsOwner)
            {
                RequestRoleSyncServerRpc(NetworkManager.Singleton.LocalClientId);
            }

            CreateRoleText();
            Role.OnValueChanged += OnRoleChanged;
            UpdateRoleText(Role.Value);

            if (IsClient && Role.Value == PlayerRole.None)
            {
                Debug.Log("Le joueur attend la sélection d'un rôle...");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestRoleSyncServerRpc(ulong clientId)
        {
            Debug.Log($"[Serveur] Synchronisation demandée par le client {clientId}");
            SyncRoleSelectionClientRpc(_specialisteSelected.Value, _technicienSelected.Value, clientId);
        }

        [ClientRpc]
        private void SyncRoleSelectionClientRpc(bool specialisteTaken, bool technicienTaken, ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                _specialisteSelected.Value = specialisteTaken;
                _technicienSelected.Value = technicienTaken;
                Debug.Log($"[Client] Synchronisation reçue : Spécialiste = {_specialisteSelected.Value}, Technicien = {_technicienSelected.Value}");
        
                // Affiche le panneau de sélection pour le client
                GameObject.FindObjectOfType<NetworkManagerRelay>()?.ShowRoleSelection();
            }
        }
        [ClientRpc]
        private void InitializePlayerClientRpc()
        {
            if (IsOwner)
            {
                Move();
            }
        }

        private void CreateRoleText()
        {
            GameObject textObject = new GameObject("RoleText");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = new Vector3(0, 2, 0);
            roleText = textObject.AddComponent<TextMeshPro>();
            roleText.alignment = TextAlignmentOptions.Center;
            roleText.fontSize = 3;
            roleText.color = Color.white;
        }

        private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
        {
            UpdateRoleText(newValue);
            if (IsServer)
            {
                
                switch (newValue)
                {
                    case PlayerRole.Specialiste:
                        _specialisteSelected.Value = true;
                        break;
                    case PlayerRole.Technicien:
                        _technicienSelected.Value = true;
                        break;
                }
            }
        }

        private void UpdateRoleText(PlayerRole role)
        {
            if (roleText != null)
            {
                roleText.text = role.ToString();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetRoleServerRpc(PlayerRole newRole, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if (!CanSelectRole(newRole))
            {
                Debug.LogWarning($"[Serveur] Le joueur {clientId} a essayé de prendre {newRole}, mais il est déjà pris.");
                return;
            }

            Role.Value = newRole;
            Debug.Log($"[Serveur] Rôle {newRole} attribué au joueur {clientId}");

            switch (newRole)
            {
                case PlayerRole.Specialiste:
                    _specialisteSelected.Value = true;
                    break;
                case PlayerRole.Technicien:
                    _technicienSelected.Value = true;
                    break;
            }
        }

        private bool CanSelectRole(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Specialiste => !_specialisteSelected.Value,
                PlayerRole.Technicien => !_technicienSelected.Value,
                _ => false
            };
        }

        public void Move()
        {
            if (Role.Value == PlayerRole.None)
            {
                Debug.LogWarning("Player role not selected yet!");
                return;
            }
            SubmitPositionRequestServerRpc();
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc()
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        void Update()
        {
            if (IsSpawned)
            {
                transform.position = Position.Value;
                if (roleText != null && Camera.main != null)
                {
                    roleText.transform.forward = Camera.main.transform.forward;
                }
            }
        }
    }
}