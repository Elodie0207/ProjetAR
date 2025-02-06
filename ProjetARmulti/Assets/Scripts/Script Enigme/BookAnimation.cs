using UnityEngine;

public class BookAnimation : MonoBehaviour
{
    public Animator[] pageAnimators; // Tableau d'animateurs
    private int currentPageIndex = -1;

    void OnMouseDown()
    {
        // Vérifie si le tableau n'est pas vide
        if (pageAnimators != null && pageAnimators.Length > 0)
        {
            // Incrémente l'index de page
            currentPageIndex = (currentPageIndex + 1) % pageAnimators.Length;

            // Déclenche l'animation de la page courante
            pageAnimators[currentPageIndex].SetTrigger("TurnPage");
        }
    }
}