using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Input3DHandler : MonoBehaviour
{
    public TMP_InputField inputField;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Clique gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform) // Si on clique sur le texte
                {
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
        }
    }
}