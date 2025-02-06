using UnityEngine;
using UnityEngine.UI;

public class NeedleVisibilty : MonoBehaviour
{
    public GameObject object1;  // Premier objet à activer (Petite Aiguille)
    public GameObject object2;  // Deuxième objet à activer (Grande Aiguille)
    public GameObject object3;  // Troisième objet à activer quand l'heure est correcte
    public string targetTag = "ImageTarget";  // Tag de l'Image Target
    public string tag12 = "12";  // Tag pour la position "12"
    public string tag6 = "6";    // Tag pour la position "6"
    
    public Text timeDisplay;     // Texte affichant l'heure (format 00:00)
    private float timeInMinutes = 0f;  // Compteur d'heure en minutes (0 à 720 minutes pour 12:00)
    
   

    // Mise à jour de l'affichage de l'heure
    private void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(timeInMinutes) % 60;
        int hours = Mathf.FloorToInt(timeInMinutes) / 60;

        timeDisplay.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
    }

    
    private void CheckTime()
    {
        
        if (timeInMinutes == 750f) 
        {
            if (object2 != null)
                object2.SetActive(false);  
            
            if (object3 != null)
                object3.SetActive(true); 
            Debug.Log("Heure correcte (12:30) atteinte, activation de l'objet3 !");
        }
    }


    void OnTriggerEnter(Collider other)
    {
    
        if (other.CompareTag(targetTag))
        {
          
            if (gameObject.CompareTag("Petite Aiguille"))
            {
                if (object1 != null)
                {
                    object1.SetActive(true);  // Active l'objet 1
                }
                Debug.Log("Petite Aiguille a touché l'ImageTarget et activé l'objet1");
            }
            else if (gameObject.CompareTag("Grande Aiguille"))
            {
                if (object2 != null)
                {
                    object2.SetActive(true);  // Active l'objet 2
                }
                Debug.Log("Grande Aiguille a touché l'ImageTarget et activé l'objet2");
            }
        }

        
        if (gameObject.CompareTag("Petite Aiguille") && other.CompareTag(tag12))
        {
            Debug.Log("Petite Aiguille a touché le tag 12, position correcte !");
      
            ValidateTime();
        }

     
        if (gameObject.CompareTag("Grande Aiguille") && other.CompareTag(tag6))
        {
            Debug.Log("Grande Aiguille a touché le tag 6, position correcte !");
         
            ValidateTime();
        }
    }

  
    void ValidateTime()
    {
       
        Debug.Log("L'heure est correcte: 12:30 !");
    }
}
