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
        Debug.Log("jejjee");
        // Vérifie le nom du GameObject qui a été cliqué
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
        timeInMinutes += 5f;  // Incrémenter de 5 minutes
        if (timeInMinutes > 720f) // Limiter l'heure à 12:00 (720 minutes)
            timeInMinutes = 720f;

        UpdateTimeDisplay();
        CheckTime();
    }

    // Fonction pour diminuer l'heure
    private void DecreaseTime()
    {
        timeInMinutes -= 5f;  // Diminuer de 5 minutes
        if (timeInMinutes < 0f) // Limiter l'heure à 00:00
            timeInMinutes = 0f;

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

    // Vérifie si l'heure est correcte (12:30)
    private void CheckTime()
    {
        // Si l'heure atteint 12:30 (750 minutes), faire les changements d'activation
        if (timeInMinutes == 750f) // 12:30 = 12 * 60 + 30 = 750 minutes
        {
            if (object2 != null)
                object2.SetActive(false);  // Désactiver l'objet2 (grande aiguille)

            if (object3 != null)
                object3.SetActive(true);  // Activer l'objet3
            Debug.Log("Heure correcte (12:30) atteinte, activation de l'objet3 !");
        }
    }
}
