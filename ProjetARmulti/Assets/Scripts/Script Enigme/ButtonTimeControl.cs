using UnityEngine;
using UnityEngine.UI;

public class ButtonTimeControl : MonoBehaviour
{
    public GameObject object2;  // Deuxième objet à activer (Grande Aiguille)
    public GameObject object3;  // Troisième objet à activer quand l'heure est correcte
    public TextMesh timeDisplay;  // Texte affichant l'heure (format 00:00)

    private static float timeInMinutes = 0f;  // Compteur d'heure en minutes (0 à 720 minutes pour 12:00)

    public Button hautButton;  // Référence au bouton Haut
    public Button basButton;   // Référence au bouton Bas

    // Start est appelé au début
    void Start()
    {
        // Ajoute les écouteurs pour les clics des boutons (UI)
        hautButton.onClick.AddListener(IncreaseTime);
        basButton.onClick.AddListener(DecreaseTime);
    }

    // Fonction appelée quand on clique sur le bouton via OnMouseDown()
    void OnMouseDown()
    {
        if (object2 == null || !object2.activeSelf) return;
        
        if (gameObject.name == "Haut")
        {
            IncreaseTime();  // Augmenter l'heure si c'est le bouton "Haut"
        }
        else if (gameObject.name == "Bas")
        {
            DecreaseTime();  // Diminuer l'heure si c'est le bouton "Bas"
        }
    }

    // Fonction pour augmenter l'heure
    private void IncreaseTime()
    {
        if (object2 == null || !object2.activeSelf) return; // Vérifie si object2 est actif avant de modifier le temps

        timeInMinutes += 5f;
        timeInMinutes = Mathf.Clamp(timeInMinutes, 0f, 720f);

        UpdateTimeDisplay();
        CheckTime();
    }

    // Fonction pour diminuer l'heure
    private void DecreaseTime()
    {
        if (object2 == null || !object2.activeSelf) return; // Vérifie si object2 est actif avant de modifier le temps

        timeInMinutes -= 5f;
        timeInMinutes = Mathf.Clamp(timeInMinutes, 0f, 720f);

        UpdateTimeDisplay();
        CheckTime();
    }

    // Mise à jour de l'affichage de l'heure
    private void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(timeInMinutes) % 60;
        int hours = Mathf.FloorToInt(timeInMinutes) / 60;

        timeDisplay.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
    }

    // Vérifie si l'heure est correcte (06:00)
    private void CheckTime()
    {
        if (timeInMinutes == 360f) // 06:00 = 6 * 60 = 360 minutes
        {
            if (object2 != null)
                object2.SetActive(false);  // Désactiver l'objet2 (grande aiguille)

            if (object3 != null)
                object3.SetActive(true);  // Activer l'objet3
            
            Debug.Log("Heure correcte (06:00) atteinte, activation de l'objet3 !");
        }
    }
}
