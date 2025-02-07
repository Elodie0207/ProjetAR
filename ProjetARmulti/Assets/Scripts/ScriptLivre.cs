using UnityEngine;
using UnityEngine.UI; // Pour le Slider

public class ScriptLivre : MonoBehaviour
{
    [SerializeField] private GameObject[] pages; 
    [SerializeField] private Slider sliderPages; 
    
    private void Start()
    {
       
        if (sliderPages != null)
        {
            sliderPages.minValue = 0;
            sliderPages.maxValue = pages.Length - 1;
            sliderPages.wholeNumbers = true; 
            
            sliderPages.onValueChanged.AddListener(ChangerPage);
        }
        
      
        ChangerPage(0);
    }
    
    private void ChangerPage(float valeurSlider)
    {
        int pageIndex = Mathf.RoundToInt(valeurSlider);
        
      
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(false);
            }
        }
        
       
        if (pageIndex >= 0 && pageIndex < pages.Length && pages[pageIndex] != null)
        {
            pages[pageIndex].SetActive(true);
        }
    }
}