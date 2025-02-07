using Unity.Netcode;
using UnityEngine;

public class BookAnimationController : NetworkBehaviour
{
    // Structure simple pour lier un objet 3D à son animation
    [System.Serializable]
    public class PageSetup
    {
        public GameObject pageObject;
        public AnimationClip animationClip;
    }

    [SerializeField] private PageSetup[] pages;
    private NetworkVariable<int> currentIndex = new NetworkVariable<int>(0);

    private void Start()
    {
        foreach (var page in pages)
        {
            if (page.pageObject != null)
            {
                var anim = page.pageObject.GetComponent<Animation>();
                if (anim == null)
                    anim = page.pageObject.AddComponent<Animation>();

                if (page.animationClip != null)
                {
                    page.animationClip.legacy = true;
                    anim.AddClip(page.animationClip, page.animationClip.name);
                }
            }
        }
    }

    private void Update()
    {
        if (IsOwner && Input.GetMouseButtonDown(0))
        {
            PlayNextAnimationServerRpc();
        }
    }

    [ServerRpc]
    private void PlayNextAnimationServerRpc()
    {
        if (currentIndex.Value >= pages.Length)
            currentIndex.Value = 0;

        PlayAnimationClientRpc(currentIndex.Value);
        currentIndex.Value++;
    }

    [ClientRpc]
    private void PlayAnimationClientRpc(int index)
    {
        if (index < pages.Length && pages[index].pageObject != null)
        {
            var anim = pages[index].pageObject.GetComponent<Animation>();
            if (anim != null && pages[index].animationClip != null)
            {
                anim.Play(pages[index].animationClip.name);
                Debug.Log($"Playing animation: {pages[index].animationClip.name}");
            }
        }
    }
}