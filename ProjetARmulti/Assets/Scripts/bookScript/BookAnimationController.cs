using UnityEngine;

public class BookAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class PageSetup
    {
        public GameObject pageObject;
        public AnimationClip animationClip;
    }

    [SerializeField] private PageSetup[] pages;
    private int currentIndex = 0;

    private void Start()
    {
        // Configuration initiale des animations
        foreach (var page in pages)
        {
            if (page.pageObject != null && page.animationClip != null)
            {
                var anim = page.pageObject.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = page.pageObject.AddComponent<Animation>();
                    Debug.Log($"Added Animation component to {page.pageObject.name}");
                }

                // Forcer le mode Legacy
                page.animationClip.legacy = true;

                // Retirer et rajouter l'animation pour être sûr
                if (anim.GetClip(page.animationClip.name))
                {
                    anim.RemoveClip(page.animationClip.name);
                }
                anim.AddClip(page.animationClip, page.animationClip.name);

                // Configurer l'animation pour qu'elle joue correctement
                anim.playAutomatically = false;
                anim.wrapMode = WrapMode.Once;

                Debug.Log($"Configured animation {page.animationClip.name} for {page.pageObject.name}");
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayNextAnimation();
        }
    }

    private void PlayNextAnimation()
    {
        if (currentIndex >= pages.Length)
        {
            currentIndex = 0;
        }

        if (currentIndex < pages.Length && pages[currentIndex].pageObject != null)
        {
            var page = pages[currentIndex];
            var anim = page.pageObject.GetComponent<Animation>();

            if (anim != null && page.animationClip != null)
            {
                // Forcer l'arrêt de toute animation en cours
                anim.Stop();

                // Jouer la nouvelle animation
                anim[page.animationClip.name].wrapMode = WrapMode.Once;
                anim[page.animationClip.name].speed = 1;
                anim.Play(page.animationClip.name);

                Debug.Log($"Playing animation: {page.animationClip.name} on {page.pageObject.name}");

                // Vérifier si l'animation joue réellement
                if (anim.isPlaying)
                {
                    Debug.Log($"Animation is successfully playing on {page.pageObject.name}");
                }
                else
                {
                    Debug.LogWarning($"Failed to play animation on {page.pageObject.name}");
                }
            }
        }

        currentIndex++;
    }
}