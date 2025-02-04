using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignauxLumineux : MonoBehaviour
{
    private static List<Color> lightSequence = new List<Color>();
    private static List<Color> playerSequence = new List<Color>();
    private static bool isShowingSequence = false;
    private static bool gameStarted = false;
    private static bool gameOver = false;

    private Renderer buttonRenderer;
    private Color originalColor;
    private static readonly int maxSequenceLength = 4;

    // Référence publique pour le GameManager et le chronomètre
    public GameManager gameManager; // Drag and drop le GameManager dans l'inspecteur

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        originalColor = buttonRenderer.material.color;
    }

    void GenerateRandomSequence()
    {
        lightSequence.Clear();
        playerSequence.Clear();
        int sequenceLength = Random.Range(1, maxSequenceLength + 1);
        
        SignauxLumineux[] allButtons = FindObjectsOfType<SignauxLumineux>();
        List<Color> availableColors = new List<Color>();

        foreach (var button in allButtons)
        {
            if (button.gameObject.name != "Start")
            {
                availableColors.Add(button.GetComponent<Renderer>().material.color);
            }
        }

        for (int i = 0; i < sequenceLength; i++)
        {
            int randomIndex = Random.Range(0, availableColors.Count);
            lightSequence.Add(availableColors[randomIndex]);
        }
        
        Debug.Log($"Nouvelle séquence générée : {sequenceLength} couleurs");
    }

    void OnMouseDown()
    {
        if (gameObject.name == "Start")
        {
            if (!isShowingSequence && !gameStarted)
            {
                Debug.Log("Start cliqué - Nouvelle partie !");
                gameStarted = true;
                gameOver = false;
                GenerateRandomSequence();
                StartCoroutine(ShowSequence());
            }
            return;
        }

        if (isShowingSequence || !gameStarted || gameOver) return;

        Color clickedColor = buttonRenderer.material.color;
        Debug.Log($"Bouton cliqué : {clickedColor}");

        StartCoroutine(FlashButton());

        playerSequence.Add(clickedColor);

        CheckSequence();
    }

    void CheckSequence()
    {
        int index = playerSequence.Count - 1;

        if (playerSequence[index] != lightSequence[index])
        {
            Debug.Log("Erreur - Partie perdue !");
            GameOver(false);
            return;
        }

        if (playerSequence.Count == lightSequence.Count)
        {
            Debug.Log("Séquence réussie !");
            GameOver(true);
        }
    }

    void GameOver(bool win)
    {
        gameOver = true;
        gameStarted = false;

        if (win)
        {
            Debug.Log("Vous avez gagné ! Le jeu est terminé.");
            DisableStartButton();
        }
        else
        {
            Debug.Log("Vous avez perdu. Cliquez sur Start pour recommencer.");
            // Réduire le temps du chrono de 2 minutes (120 secondes) en cas d'erreur
            if (gameManager != null)
            {
                gameManager.ReduceTime(120f); // Réduction du temps de 2 minutes
            }
        }
    }

    void DisableStartButton()
    {
        SignauxLumineux[] allButtons = FindObjectsOfType<SignauxLumineux>();
        foreach (var button in allButtons)
        {
            if (button.gameObject.name == "Start")
            {
                button.GetComponent<Collider>().enabled = false;
                break;
            }
        }
    }

    IEnumerator ShowSequence()
    {
        isShowingSequence = true;
        playerSequence.Clear();
        yield return new WaitForSeconds(0.5f);

        SignauxLumineux[] allButtons = FindObjectsOfType<SignauxLumineux>();

        foreach (Color sequenceColor in lightSequence)
        {
            foreach (var button in allButtons)
            {
                if (button.gameObject.name != "Start" && 
                    button.GetComponent<Renderer>().material.color == sequenceColor)
                {
                    yield return StartCoroutine(button.FlashButton());
                    break;
                }
            }
            yield return new WaitForSeconds(0.3f);
        }

        isShowingSequence = false;
        Debug.Log("Reproduisez la séquence !");
    }

    IEnumerator FlashButton()
    {
        Color startColor = buttonRenderer.material.color;
        buttonRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.3f);
        buttonRenderer.material.color = startColor;
    }
}
