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
        public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>();
        private TextMeshPro roleText;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }

            if (IsSpawned) // Vérification supplémentaire
            {
                CreateRoleText();
                Role.OnValueChanged += OnRoleChanged;
                UpdateRoleText(Role.Value);
            }
        }

        private void CreateRoleText()
        {
            // Créer un nouvel objet pour le texte
            GameObject textObject = new GameObject("RoleText");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = new Vector3(0, 2, 0); // Position au-dessus du joueur
            
            // Ajouter et configurer le TextMeshPro
            roleText = textObject.AddComponent<TextMeshPro>();
            roleText.alignment = TextAlignmentOptions.Center;
            roleText.fontSize = 3;
            roleText.color = Color.white;
        }

        private void OnRoleChanged(PlayerRole previousValue, PlayerRole newValue)
        {
            UpdateRoleText(newValue);
        }

        private void UpdateRoleText(PlayerRole role)
        {
            if (roleText != null)
            {
                roleText.text = role.ToString();
            }
        }

        [ServerRpc]
        public void SetRoleServerRpc(PlayerRole newRole)
        {
            Role.Value = newRole;
        }

        public void Move()
        {
            if (Role.Value == PlayerRole.None)
            {
                Debug.LogWarning("Player role not selected yet!");
                return;
            }
            SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        void SubmitPositionRequestRpc(RpcParams rpcParams = default)
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
            if (IsSpawned) // Vérifier que le network est actif
            {
                // Vérifier que Position n'est pas null avant d'accéder à sa valeur
                if (Position != null)
                {
                    transform.position = Position.Value;
                }

                // Vérifier que roleText n'est pas null avant d'essayer de le faire face à la caméra
                if (roleText != null && Camera.main != null)
                {
                    roleText.transform.forward = Camera.main.transform.forward;
                }
            }
        }
    }
}