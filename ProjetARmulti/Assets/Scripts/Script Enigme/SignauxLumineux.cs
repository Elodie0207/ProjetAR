using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignauxLumineux : MonoBehaviour
{
    // Liste de tous les boutons de la scène
    private Renderer[] buttonRenderers;
    private Material[] buttonMaterials;  // Matériaux des boutons

    // Liste pour stocker la séquence des boutons allumés
    private List<int> lightSequence = new List<int>();
    private int sequenceIndex = 0;

    // Variable pour indiquer si le jeu est en train de montrer la séquence ou si l'utilisateur peut cliquer
    private bool isShowingSequence = true;

    // Start is called before the first frame update
    void Start()
    {
        // Trouver tous les objets avec un Renderer dans la scène qui sont des boutons
        buttonRenderers = FindObjectsOfType<Renderer>();
        buttonMaterials = new Material[buttonRenderers.Length];

        // Sauvegarder tous les matériaux originaux
        for (int i = 0; i < buttonRenderers.Length; i++)
        {
            buttonMaterials[i] = buttonRenderers[i].material;
            // Désactiver l'émission au départ
            buttonRenderers[i].material.SetColor("_EmissionColor", Color.black);
        }

        // Démarrer une nouvelle séquence aléatoire
        GenerateRandomSequence();
        StartCoroutine(ShowSequence());
    }

    // Générer une séquence de boutons allumés de manière aléatoire
    void GenerateRandomSequence()
    {
        lightSequence.Clear();
        int sequenceLength = 5; // La longueur de la séquence peut être ajustée

        for (int i = 0; i < sequenceLength; i++)
        {
            int randomIndex = Random.Range(0, buttonRenderers.Length); // Choisit un bouton au hasard
            lightSequence.Add(randomIndex);
        }
    }

    // Afficher la séquence de manière visuelle (avec un délai entre chaque bouton)
    IEnumerator ShowSequence()
    {
        // Nous affichons la séquence un par un
        foreach (int index in lightSequence)
        {
            // Éteindre tous les boutons avant d'allumer le suivant
            foreach (var buttonRenderer in buttonRenderers)
            {
                buttonRenderer.material.SetColor("_EmissionColor", Color.black); // Désactive l'émission des autres boutons
            }

            Material clickedMaterial = buttonMaterials[index];
            Color buttonColor = clickedMaterial.color;

            // Applique l'émission pour le bouton
            clickedMaterial.SetColor("_EmissionColor", buttonColor * 2f); // Intensité de l'émission

            clickedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Attendre un moment avant d'éteindre l'émission et passer au suivant
            yield return new WaitForSeconds(0.5f); 

            // Éteindre l'émission après le délai
            clickedMaterial.SetColor("_EmissionColor", Color.black);
        }

        // Une fois la séquence affichée, l'utilisateur peut maintenant commencer à cliquer
        Debug.Log("Cliquez sur les boutons dans le bon ordre !");
        isShowingSequence = false; // Permet à l'utilisateur de cliquer
    }

    // Cette fonction sera appelée quand l'utilisateur clique sur un bouton
    public void OnMouseDown()
    {
        if (isShowingSequence) return; // Ignore les clics pendant l'affichage de la séquence

        // Affiche le nom de l'objet cliqué dans la console
        Debug.Log(gameObject.name + " cliqué !");

        Renderer clickedButtonRenderer = GetComponent<Renderer>();
        int clickedIndex = System.Array.IndexOf(buttonRenderers, clickedButtonRenderer);

        // Vérifier si l'utilisateur a cliqué sur le bon bouton
        if (clickedIndex == lightSequence[sequenceIndex])
        {
            // Si le bouton cliqué est correct, augmente l'index de la séquence
            sequenceIndex++;
            
            // Vérifie si toute la séquence a été réussie
            if (sequenceIndex >= lightSequence.Count)
            {
                Debug.Log("gg! Vous avez réussi !");
                sequenceIndex = 0; // Réinitialiser la séquence pour recommencer
                GenerateRandomSequence(); // Générer une nouvelle séquence
                StartCoroutine(ShowSequence()); // Afficher la nouvelle séquence
            }
        }
        else
        {
            // Si l'utilisateur se trompe
            Debug.Log("Perdu! Essayez encore.");
            sequenceIndex = 0; // Réinitialiser la séquence
            GenerateRandomSequence(); // Générer une nouvelle séquence
            StartCoroutine(ShowSequence()); // Afficher la nouvelle séquence
        }

        // Applique l'effet lumineux sur l'objet cliqué
        if (clickedButtonRenderer != null)
        {
            Material clickedMaterial = clickedButtonRenderer.material;
            Color buttonColor = clickedMaterial.color;

            // Vérifie si la couleur est bordeaux (une couleur avec beaucoup de rouge et une faible composante verte/bleue)
            if (buttonColor.r > 0.5f && buttonColor.g < 0.2f && buttonColor.b < 0.2f)
            {
                clickedMaterial.SetColor("_EmissionColor", buttonColor * 5f); // Intensité d'émission plus forte pour le bordeaux
            }
            else
            {
                clickedMaterial.SetColor("_EmissionColor", buttonColor * 2f);
            }

            clickedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        // Si tu veux éteindre l'émission des autres boutons (si un seul bouton doit briller à la fois)
        foreach (Renderer buttonRenderer in buttonRenderers)
        {
            if (buttonRenderer != clickedButtonRenderer)
            {
                buttonRenderer.material.SetColor("_EmissionColor", Color.black); // Désactive l'émission des autres
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Optionnel : rendre l'émission plus visible sous des éclairages spécifiques
        foreach (Renderer buttonRenderer in buttonRenderers)
        {
            buttonRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }
}
