using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class GameManager : MonoBehaviour
{
    public Text timerText; 
    private float timeRemaining = 15 * 60; 


    void Start()
    {
        
    }

   
    void Update()
    {
            Timer();
    }
    

    void Timer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
        }

     
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
    }

    void Tests()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerText n'est pas assigné dans l'inspecteur !");
        }
        
        
    }
}