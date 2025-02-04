using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public float timeRemaining = 900f; // 15 minutes en secondes
    public bool timerIsRunning = false;
    public TextMesh timerText; // Optionnel : Pour afficher le temps dans une UI

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
    
    public void ReduceTime(float seconds)
    {
        timeRemaining -= seconds;
        if (timeRemaining < 0) timeRemaining = 0; // Ne pas laisser le temps aller en dessous de 0
    }
}
