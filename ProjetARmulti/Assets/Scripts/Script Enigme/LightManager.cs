using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public Material LightRed;
    public Material LightGreen;
    public Material LightWhite;

    private bool isPressed = false;
    private bool inDelay = false;
    
    public void Play (){
        

        if (isPressed == false ) { isPressed = true; }
        
        else if (isPressed == true && inDelay == true)
        {
            print("victoire");
            return;
        }
        else if (isPressed == true && inDelay == false) { return; }



        //List<MeshRenderer> Lights = new List<MeshRenderer>();
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

        isPressed = false;
    }




}
