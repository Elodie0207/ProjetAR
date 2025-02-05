using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorseScript : MonoBehaviour
{
    public GameManager gameManager; 

    private void OnMouseDown()
    {
        if (gameObject.name.ToLower().Contains("gauche"))
        {
            Debug.Log("Réponse correcte !");
        }
        else if (gameObject.name.ToLower().Contains("droite"))
        {
            Debug.Log("Réponse incorrecte ! Réduction de 6 minutes.");
            if (gameManager != null)
            {
                gameManager.ReduceTime(360f); 
            }
        }
    }
}
