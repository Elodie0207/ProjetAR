using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    public LightManager lightManager;

    private void OnMouseDown()
    {
        lightManager.Play();
    }
}
