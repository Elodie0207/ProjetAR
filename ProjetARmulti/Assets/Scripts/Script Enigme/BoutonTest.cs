using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoutonTest : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public LightManager lightManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Bouton appuyé"); // Pour débugger
        lightManager.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Bouton cliqué"); // Pour débugger
        lightManager.Play();
    }
}