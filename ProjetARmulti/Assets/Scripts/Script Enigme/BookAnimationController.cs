using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BookAnimationController : MonoBehaviour
{
    private Animator[] pageAnimators;
    private int currentPageIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        // Récupère tous les Animators des enfants
        Transform[] children = GetComponentsInChildren<Transform>();
        // -1 car on ne compte pas le parent bookV5
        pageAnimators = new Animator[transform.childCount - 1];

        int index = 0;
        foreach (Transform child in transform)
        {
            // Ignore l'objet parent lui-même
            if (child != transform)
            {
                Animator animator = child.GetComponent<Animator>();
                if (animator != null)
                {
                    pageAnimators[index] = animator;
                    index++;
                    Debug.Log("Found animator on: " + child.name); // Debug
                }
                else
                {
                    Debug.LogWarning("No Animator found on: " + child.name); // Debug
                }
            }
        }

        Debug.Log("Total animators found: " + index); // Debug
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider != null)
            {
                if (hit.transform == this.transform || hit.transform.parent == this.transform)
                {
                    PlayNextAnimation();
                }
            }
        }
    }

    void PlayNextAnimation()
    {
        if (currentPageIndex < pageAnimators.Length && pageAnimators[currentPageIndex] != null)
        {
            Debug.Log("Playing animation for page: " + currentPageIndex); // Debug

            // Déclenche l'animation
            Animator currentAnimator = pageAnimators[currentPageIndex];
            currentAnimator.Play("Base Layer.Default", 0, 0f);

            isAnimating = true;
            StartCoroutine(WaitForAnimationEnd(currentAnimator));

            currentPageIndex++;
        }
    }

    IEnumerator WaitForAnimationEnd(Animator animator)
    {
        // Attend que l'animation se termine
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAnimating = false;
    }
}