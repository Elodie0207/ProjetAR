using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public Material LightRed;
    public Material LightGreen;
    public Material LightWhite;

    private bool isPressed = false;
    private bool inDelay = false;

    // Référence au GameManager
    public GameManager gameManager; // À lier dans l'inspecteur

    public void Play()
    {
        if (isPressed == false)
        {
            isPressed = true;
        }
        else if (isPressed == true && inDelay == true)
        {
            print("victoire");
            return;
        }
        else if (isPressed == true && inDelay == false) 
        { 
            return; 
        }

        // On continue si le bouton n'est pas déjà pressé et qu'il n'est pas en délai
        MeshRenderer[] lights;
        lights = GetComponentsInChildren<MeshRenderer>();

        // Lancement de la coroutine pour afficher la lumière rouge
        StartCoroutine(DelayLightRed(lights));
    }

    private void OnMouseDown()
    {
        Play();
    }

    private IEnumerator DelayLightRed(MeshRenderer[] lights)
    {
        // Allume la lumière rouge pendant 1 seconde
        foreach (MeshRenderer light in lights)
        {
            yield return new WaitForSeconds(1);
            light.material = LightRed;
        }

        yield return new WaitForSeconds(3);  // Temps en rouge (3 secondes)

        // Allume la lumière verte
        foreach (MeshRenderer light in lights)
        {
            light.material = LightGreen;
        }
        inDelay = true;

        yield return new WaitForSeconds(1);  // Temps avec la lumière verte

        // Éteint les lumières et les remet en blanc
        foreach (MeshRenderer light in lights)
        {
            light.material = LightWhite;
        }

        inDelay = false;

        // Si la lumière était rouge, on retire 2 minutes
        if (lights[0].material == LightRed)
        {
            // Si c'est rouge, on retire 2 minutes
            if (gameManager != null)
            {
                gameManager.ReduceTime(120f); // Réduit le temps de 2 minutes
            }
        }

        // Réinitialisation de l'état
        isPressed = false;
    }
}
