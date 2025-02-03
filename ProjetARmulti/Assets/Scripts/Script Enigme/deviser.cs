using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deviser : MonoBehaviour
{
    [System.Serializable]
    public class ScrewConfig
    {
        public string name;
        public BoxCollider screwZone;
        public GameObject screwObject;
        public bool isUnscrewed = false;
    }

    [Header("Screw Configuration")]
    public ScrewConfig topLeftScrew = new ScrewConfig { name = "Top Left Screw" };
    public ScrewConfig topRightScrew = new ScrewConfig { name = "Top Right Screw" };
    public ScrewConfig bottomLeftScrew = new ScrewConfig { name = "Bottom Left Screw" };
    public ScrewConfig bottomRightScrew = new ScrewConfig { name = "Bottom Right Screw" };

    [Header("Screwdriver")]
    public Transform screwdriverPosition;

    [Header("Plate")]
    public GameObject plate;

    private int unscrewedScrews = 0;

    private void Update()
    {
        CheckScrewdriverProximity(topLeftScrew);
        CheckScrewdriverProximity(topRightScrew);
        CheckScrewdriverProximity(bottomLeftScrew);
        CheckScrewdriverProximity(bottomRightScrew);
    }

    private void CheckScrewdriverProximity(ScrewConfig screw)
    {
        if (screw.isUnscrewed || screw.screwZone == null || screwdriverPosition == null)
            return;

        if (screw.screwZone.bounds.Contains(screwdriverPosition.position))
        {
            screw.isUnscrewed = true;
            screw.screwObject.SetActive(false);
            unscrewedScrews++;
            Debug.Log(screw.name + " unscrewed!");

            if (unscrewedScrews >= 4)
            {
                Debug.Log("All screws unscrewed!");
                plate.SetActive(false);
            }
        }
    }

}
