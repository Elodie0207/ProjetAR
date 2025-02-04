using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NumericPuzzle : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject[] numberButtons;
    public TextMeshPro[] numberDisplays;

    [Header("Correct Code")]
    public int[] correctCode = { 8, 1, 6, 4 };

    private int[] currentCode = new int[4];
    private int currentPosition = 0;

    private void Start()
    {
        for (int i = 0; i < currentCode.Length; i++)
        {
            currentCode[i] = 0;
            if (numberDisplays[i] != null)
            {
                numberDisplays[i].text = "0";
            }
        }

        if (numberDisplays[0] != null)
        {
            numberDisplays[0].color = new Color32(131, 117, 215, 255); // Violet
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
                for (int i = 0; i < numberButtons.Length; i++)
                {
                    if (hit.collider.gameObject == numberButtons[i])
                    {
                        HandleNumberButtonPress(i);
                        break;
                    }
                }
            }
        }
    }

    private void HandleNumberButtonPress(int buttonIndex)
    {
        if (currentPosition >= currentCode.Length) return;

        currentCode[currentPosition] = buttonIndex + 1;
        UpdateCurrentDisplay(buttonIndex + 1);

        currentPosition++;

        if (currentPosition < currentCode.Length)
        {
            if (numberDisplays[currentPosition] != null)
            {
                switch (currentPosition)
                {
                    case 0:
                        numberDisplays[currentPosition].color = new Color32(131, 117, 215, 255); // Violet
                        break;
                    case 1:
                        numberDisplays[currentPosition].color = new Color32(124, 230, 151, 255); // Vert
                        break;
                    case 2:
                        numberDisplays[currentPosition].color = new Color32(234, 143, 212, 255); // Rose
                        break;
                    case 3:
                        numberDisplays[currentPosition].color = new Color32(247, 171, 102, 255); // Orange
                        break;
                }
            }
        }
        else
        {
            CheckCode();
        }
    }

    private void CheckCode()
    {
        bool isCorrect = true;
        for (int i = 0; i < currentCode.Length; i++)
        {
            if (currentCode[i] != correctCode[i])
            {
                isCorrect = false;
                break;
            }
        }

        Color resultColor = isCorrect ? Color.green : Color.red;
        foreach (TextMeshPro display in numberDisplays)
        {
            display.color = resultColor;
        }

        Debug.Log(isCorrect ? "Code correct !" : "Code incorrect !");

        // Vous pouvez ajouter ici d'autres actions en fonction du résultat
        // Par exemple, ouvrir une porte si le code est correct
    }

    private void UpdateCurrentDisplay(int number)
    {
        if (currentPosition < currentCode.Length && numberDisplays[currentPosition] != null)
        {
            numberDisplays[currentPosition].text = number.ToString();
        }
    }

    public void ResetCode()
    {
        currentPosition = 0;

        for (int i = 0; i < currentCode.Length; i++)
        {
            currentCode[i] = 0;
            if (numberDisplays[i] != null)
            {
                numberDisplays[i].text = "0";
                numberDisplays[i].color = Color.white;
            }
        }

        if (numberDisplays[0] != null)
        {
            numberDisplays[0].color = new Color32(131, 117, 215, 255); // Violet
        }
    }
}