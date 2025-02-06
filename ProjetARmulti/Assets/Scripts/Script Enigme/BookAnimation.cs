using UnityEngine;

public class BookAnimation : MonoBehaviour
{
    public Animator[] pageAnimators; // Tableau d'animateurs
    private int currentPageIndex = -1;

    void OnMouseDown()
    {
        // V�rifie si le tableau n'est pas vide
        if (pageAnimators != null && pageAnimators.Length > 0)
        {
            // Incr�mente l'index de page
            currentPageIndex = (currentPageIndex + 1) % pageAnimators.Length;

            // D�clenche l'animation de la page courante
            pageAnimators[currentPageIndex].SetTrigger("TurnPage");
        }
    }
}