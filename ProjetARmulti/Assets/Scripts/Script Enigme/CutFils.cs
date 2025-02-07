using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutFils : MonoBehaviour
{
    public string color; 
    public Animator animatorGauche;
    public Animator animatorDroite;
    public GameObject filPartieGauche;
    public GameObject filPartieDroite;
    private bool estCoupe = false;

    private static bool ciseauSelectionne = false;
    public GameManager gameManager;
    private void OnMouseDown()
    {
        if (!ciseauSelectionne) return; 

        if (!estCoupe)
        {
           
         
            if (filPartieDroite != null)
                filPartieDroite.SetActive(false); 

            estCoupe = true;
            Debug.Log("Le fil " + color + " a été coupé !");
            VerifierConditionJeu();
        }
    }

    private void VerifierConditionJeu()
    {
        if (color == "noir")
        {
            if (gameManager != null)
            {
                gameManager.SetLightColor(false); // Lumière rouge
                gameManager.timeRemaining = 0f; 
            }
            Debug.Log("BOOM  ! Le fil noir était piégé !");
        }
        else if (color == "bleu")
        {
            gameManager.SetLightColor(true); // Lumière rouge
            Debug.Log(" Vous avez gagné ! C'était le bon fil !");
        }
        else
          
        {
            gameManager.ReduceTime(120f);
            gameManager.SetLightColor(false); // Lumière rouge
            Debug.Log(" Pas le bon fil !");
        }

        // Une fois le fil coupé, on désélectionne le ciseau
        ciseauSelectionne = false;
    }

    public static void SelectionnerCiseau()
    {
        ciseauSelectionne = true;
        Debug.Log(" Ciseau sélectionné !");
    }
}