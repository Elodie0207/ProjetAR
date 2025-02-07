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

    
    public GameManager gameManager;

    public void Play()
    {
        if (isPressed == false)
        {
            isPressed = true;
        }
        else if (isPressed == true && inDelay == true)
        { 
            gameManager.SetLightColor(true); 
            print("victoire");
            return;
        }
        else if (isPressed == true && inDelay == false) 
        { 
            return; 
        }

    
        MeshRenderer[] lights;
        lights = GetComponentsInChildren<MeshRenderer>();

        StartCoroutine(DelayLightRed(lights));
    }

    private void OnMouseDown()
    {
        Play();
    }

    private IEnumerator DelayLightRed(MeshRenderer[] lights)
    {
       
        foreach (MeshRenderer light in lights)
        {
            yield return new WaitForSeconds(1);
            light.material = LightRed;
        }

        yield return new WaitForSeconds(3);  

        // Allume la lumière verte
        foreach (MeshRenderer light in lights)
        {
            light.material = LightGreen;
        }
        inDelay = true;

        yield return new WaitForSeconds(1);  

       
        foreach (MeshRenderer light in lights)
        {
            light.material = LightWhite;
        }

        inDelay = false;


        if (lights[0].material == LightRed)
        {
           
            if (gameManager != null)
            {
                gameManager.SetLightColor(false); 
                gameManager.ReduceTime(120f); 
            }
        }

      
        isPressed = false;
    }
}
