using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public float timeRemaining = 900f; // 15 minutes en secondes
    public bool timerIsRunning = false;
    public TextMesh timerText; // Optionnel : Pour afficher le temps dans une UI
    public GameObject lightObject; // Référence à l'objet à colorer
    private int successfulEnigmas = 0;
    private const int TOTAL_ENIGMAS = 5;

    public AudioSource successSound;      // Son de succès (vert)
    public AudioSource failureSound;      // Son d'échec (rouge)
    public AudioSource finalVictorySound; // Son de victoire finale

    void Start()
    {
        timerIsRunning = true;


    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Temps écoulé !");
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1; // Pour éviter d'afficher 00:59 au lieu de 01:00
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Fonction pour gérer la couleur de la lumière
    public void SetLightColor(bool success)
    {
        if (lightObject != null)
        {
            Renderer renderer = lightObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                Color lightColor = success ? Color.green : Color.red;

                renderer.material.color = lightColor;

                if (success)
                {
                    successfulEnigmas++;
                    successSound.Play(); // Joue le son de succès
                    Debug.Log($"Énigmes réussies : {successfulEnigmas}/5");

                    if (successfulEnigmas >= 5)
                    {
                        Debug.Log("Victoire finale !");
                        finalVictorySound.Play(); // Joue le son de victoire finale
                        // Actions supplémentaires de victoire
                    }
                }
                else
                {
                    failureSound.Play(); // Joue le son d'échec
                    successfulEnigmas = 0; // Réinitialiser en cas d'échec
                }

                StartCoroutine(ResetLightColor(renderer, originalColor, 3f));
            }
        }
    }


private IEnumerator ResetLightColor(Renderer renderer, Color originalColor, float duration)
    {
        yield return new WaitForSeconds(duration);
        renderer.material.color = originalColor;
    }

    public void ReduceTime(float seconds)
    {
        timeRemaining -= seconds;
        if (timeRemaining < 0) timeRemaining = 0; // Ne pas laisser le temps aller en dessous de 0
    }
}
