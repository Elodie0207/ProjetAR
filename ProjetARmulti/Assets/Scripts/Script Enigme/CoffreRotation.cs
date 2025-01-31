using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CoffreRotation : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject targetObject;
    public TextMeshPro[] numberDisplays = new TextMeshPro[5];
    public float rotationSpeed = 36f;

    [Header("Code Secret")]
    public int[] correctCode = new int[5]; // Le code correct � d�finir dans l'Inspector

    [Header("Contr�les")]
    public Collider clockwiseButton;
    public Collider counterClockwiseButton;
    public Collider dialCollider;

    private int currentNumber = 0;
    private int currentPosition = 0;
    private int[] savedNumbers = new int[5];

    private void Start()
    {
        if (targetObject == null) targetObject = gameObject;

        for (int i = 0; i < savedNumbers.Length; i++)
        {
            savedNumbers[i] = 0;
            if (numberDisplays[i] != null)
            {
                numberDisplays[i].text = "0";
            }
        }

        if (numberDisplays[0] != null)
        {
            numberDisplays[0].color = Color.yellow;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == clockwiseButton)
                {
                    RotateClockwise();
                }
                else if (hit.collider == counterClockwiseButton)
                {
                    RotateCounterClockwise();
                }
                else if (hit.collider == dialCollider)
                {
                    ValidateAndMoveNext();
                }
            }
        }
    }

    private void RotateClockwise()
    {
        if (currentPosition >= 5) return;

        targetObject.transform.Rotate(0f, 0f, -rotationSpeed);
        currentNumber = (currentNumber + 1) % 10;
        UpdateCurrentDisplay();
    }

    private void RotateCounterClockwise()
    {
        if (currentPosition >= 5) return;

        targetObject.transform.Rotate(0f, 0f, rotationSpeed);
        currentNumber = (currentNumber - 1 + 10) % 10;
        UpdateCurrentDisplay();
    }

    private void ValidateAndMoveNext()
    {
        if (currentPosition >= 5) return;

        savedNumbers[currentPosition] = currentNumber;

        if (numberDisplays[currentPosition] != null)
        {
            numberDisplays[currentPosition].color = Color.white;
        }

        currentPosition++;

        if (currentPosition < 5)
        {
            if (numberDisplays[currentPosition] != null)
            {
                numberDisplays[currentPosition].color = Color.yellow;
            }
            currentNumber = savedNumbers[currentPosition];
            UpdateCurrentDisplay();
        }
        else
        {
            CheckCode();
        }
    }

    private void CheckCode()
    {
        bool isCorrect = true;
        for (int i = 0; i < 5; i++)
        {
            if (savedNumbers[i] != correctCode[i])
            {
                isCorrect = false;
                break;
            }
        }

        Color resultColor = isCorrect ? Color.green : Color.red;
        foreach (TextMeshPro display in numberDisplays)
        {
            if (display != null)
            {
                display.color = resultColor;
            }
        }

        Debug.Log(isCorrect ? "Code correct !" : "Code incorrect !");

        // Vous pouvez ajouter ici d'autres actions en fonction du r�sultat
        // Par exemple, ouvrir le coffre si le code est correct
    }

    private void UpdateCurrentDisplay()
    {
        if (currentPosition < 5 && numberDisplays[currentPosition] != null)
        {
            numberDisplays[currentPosition].text = currentNumber.ToString();
        }
    }

    public void ResetCode()
    {
        currentPosition = 0;
        currentNumber = 0;

        for (int i = 0; i < savedNumbers.Length; i++)
        {
            savedNumbers[i] = 0;
            if (numberDisplays[i] != null)
            {
                numberDisplays[i].text = "0";
                numberDisplays[i].color = Color.white;
            }
        }

        if (numberDisplays[0] != null)
        {
            numberDisplays[0].color = Color.yellow;
        }
    }
}