using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour
{
    public GameObject obj1; 
    public GameObject obj2; 

  
    void Start()
    {
      
        obj1.SetActive(false);
        obj2.SetActive(false);

  
        ActiverObjetAleatoire();
    }

   
    void ActiverObjetAleatoire()
    {
        int randomIndex = Random.Range(0, 2); 
        if (randomIndex == 0)
        {
            obj1.SetActive(true); 
            Debug.Log("Objet 1 activé");
        }
        else
        {
            obj2.SetActive(true); 
            Debug.Log("Objet 2 activé");
        }
    }
}
