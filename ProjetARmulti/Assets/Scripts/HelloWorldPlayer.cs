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
            
            // Créer et configurer le texte 3D
            CreateRoleText();
            
            // S'abonner au changement de rôle
            Role.OnValueChanged += OnRoleChanged;
            
            // Mettre à jour le texte initial
            UpdateRoleText(Role.Value);
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
            transform.position = Position.Value;
            
            // Faire face à la caméra pour le texte
            if (roleText != null)
            {
                roleText.transform.forward = Camera.main.transform.forward;
            }
        }
    }
}