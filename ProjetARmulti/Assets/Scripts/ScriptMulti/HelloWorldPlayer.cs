using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

        [Header("Scene Names")]
        [SerializeField] private string specialisteSceneName = "scene_Specialiste";
        [SerializeField] private string technicienSceneName = "scene_Technicien";

        private static NetworkVariable<bool> _specialisteSelected = new NetworkVariable<bool>(false);
        private static NetworkVariable<bool> _technicienSelected = new NetworkVariable<bool>(false);

        public override void OnNetworkSpawn()
        {
            if (IsClient && IsOwner)
            {
                // Demander la synchronisation des rôles
                RequestRoleSyncServerRpc();
            }

            CreateRoleText();
            Role.OnValueChanged += OnRoleChanged;
            UpdateRoleText(Role.Value);
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

        private void UpdateRoleText(PlayerRole role)
        {
            if (roleText != null)
            {
                roleText.text = role.ToString();
            }
        }

        private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
        {
            UpdateRoleText(newValue);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestRoleSyncServerRpc()
        {
            // Synchroniser l'état des rôles
            SyncRoleSelectionClientRpc(_specialisteSelected.Value, _technicienSelected.Value);
        }

        [ClientRpc]
        private void SyncRoleSelectionClientRpc(bool specialisteTaken, bool technicienTaken)
        {
            _specialisteSelected.Value = specialisteTaken;
            _technicienSelected.Value = technicienTaken;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetRoleServerRpc(PlayerRole newRole)
        {
            // Vérifier si le rôle peut être sélectionné
            if (!CanSelectRole(newRole))
            {
                Debug.LogWarning($"Rôle {newRole} déjà pris !");
                return;
            }

            // Définir le rôle
            Role.Value = newRole;

            // Marquer le rôle comme sélectionné
            switch (newRole)
            {
                case PlayerRole.Specialiste:
                    _specialisteSelected.Value = true;
                    break;
                case PlayerRole.Technicien:
                    _technicienSelected.Value = true;
                    break;
            }

            // Charger la scène spécifique pour chaque client
            LoadSceneForClientClientRpc(newRole, OwnerClientId);
        }

        [ClientRpc]
        private void LoadSceneForClientClientRpc(PlayerRole role, ulong clientId)
        {
            // Vérifier si ce RPC est pour ce client spécifique
            if (NetworkManager.Singleton.LocalClientId != clientId)
                return;

            // Charger la scène correspondante
            string sceneName = role == PlayerRole.Specialiste
                ? specialisteSceneName
                : technicienSceneName;

            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
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

        void Update()
        {
            if (IsSpawned)
            {
                transform.position = Position.Value;

                // Assurer que le texte de rôle fait toujours face à la caméra
                if (roleText != null && Camera.main != null)
                {
                    roleText.transform.forward = Camera.main.transform.forward;
                }
            }
        }

        public void Move()
        {
            if (Role.Value == PlayerRole.None)
            {
                Debug.LogWarning("Rôle du joueur non sélectionné !");
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
    }
}