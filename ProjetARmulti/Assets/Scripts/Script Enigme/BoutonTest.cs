using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoutonTest : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public LightManager lightManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Bouton appuy�"); // Pour d�bugger
        lightManager.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Bouton cliqu�"); // Pour d�bugger
        lightManager.Play();
    }
}