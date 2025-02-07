using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouttonDraw : MonoBehaviour
{
    public GameManager gameManager; 
    
    void OnMouseDown()
    {
        
        if (gameObject.name == "cube")
        {
            Debug.Log("GG"); 
            gameManager.SetLightColor(true);
        }
     
        else if (gameObject.name == "Rond")
        {
            gameManager.SetLightColor(false);
            gameManager.ReduceTime(360f);
        }
        else
        {
            Debug.Log("Nom inconnu"); 
        }
    }
}