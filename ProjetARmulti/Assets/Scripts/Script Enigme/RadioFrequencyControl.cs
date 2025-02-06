using UnityEngine;
using Vuforia;

public class RadioFrequencyControl : MonoBehaviour
{
    public Transform arTarget;
 
    public TextMesh frequencyText;

    public GameManager gameManager;


    private float previousRotation; 
    private float frequency = 0.0f; 
    private float rotationSpeed = 0.1f; 
    private bool isWinner = false; 
    private float timer = 0f; 
    private float targetFrequency = 105.3f; 
    private float timeToWin = 3f; 

    void Start()
    {
        if (frequencyText == null)
        {
            Debug.LogError("Frequency TextMesh is not assigned.");
        }
        if (arTarget == null)
        {
            Debug.LogError("AR Target transform is not assigned.");
        }
        
        
     
    }

    void Update()
    {
      
        if (isWinner)
        {
            return;
        }

      
        float currentRotation = arTarget.localEulerAngles.y;

       
        float rotationDelta = currentRotation - previousRotation;

        
        if (rotationDelta > 1) 
        {
            frequency += rotationSpeed;
        }
        else if (rotationDelta < -1)
        {
            frequency -= rotationSpeed;
        }

      
        frequency = Mathf.Clamp(frequency, 88.0f, 108.0f);

       
        frequencyText.text = frequency.ToString("F1");

      
        if (Mathf.Abs(frequency - targetFrequency) < 0.1f) 
        {
      
            timer += Time.deltaTime;

            
            if (timer >= timeToWin)
            {
                Debug.Log("Gagné!");
                isWinner = true;
                gameManager.SetLightColor(true);
            }
        }
        else
        {
            
            timer = 0f;
        }
        previousRotation = currentRotation;
    }
}
