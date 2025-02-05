using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engrenage : MonoBehaviour
{
    [System.Serializable]
    public class MovableGear
    {
        public string gearId;               // Identifiant de l'engrenage mobile
        public Transform imageTarget;        // L'Image Target de cet engrenage
        public bool estBienPlace = false;
    }

    [System.Serializable]
    public class GearSlot
    {
        public string slotId;
        public Transform emplacementCible;   // Position o� l'engrenage doit �tre plac�
        public string gearIdAttendu;        // ID de l'engrenage qui doit aller ici
        public bool estRempli = false;
    }

    [Header("Configuration des engrenages")]
    public MovableGear[] engrenagesMobiles;     // Les 4 engrenages � placer
    public GearSlot[] emplacements;             // Les 4 emplacements � remplir

    [Header("Param�tres de d�tection")]
    public float distanceValidation = 0.1f;     // Distance acceptable pour validation
    public float angleValidation = 15f;         // Tol�rance de rotation en degr�s

    private int nombreEngrenagesPlaces = 0;
    private bool puzzleComplete = false;

    private void Update()
    {
        if (!puzzleComplete)
        {
            VerifierPositionsEngrenages();
        }
    }

    private void VerifierPositionsEngrenages()
    {
        foreach (MovableGear gear in engrenagesMobiles)
        {
            if (!gear.estBienPlace && gear.imageTarget.gameObject.activeSelf)
            {
                foreach (GearSlot slot in emplacements)
                {
                    if (!slot.estRempli && gear.gearId == slot.gearIdAttendu)
                    {
                        VerifierAlignement(gear, slot);
                    }
                }
            }
        }
    }

    private void VerifierAlignement(MovableGear gear, GearSlot slot)
    {
        float distance = Vector3.Distance(gear.imageTarget.position, slot.emplacementCible.position);
        float angleDiff = Quaternion.Angle(gear.imageTarget.rotation, slot.emplacementCible.rotation);

        if (distance < distanceValidation && angleDiff < angleValidation)
        {
            ValiderPlacement(gear, slot);
        }
    }

    private void ValiderPlacement(MovableGear gear, GearSlot slot)
    {
        gear.estBienPlace = true;
        slot.estRempli = true;
        nombreEngrenagesPlaces++;

        Debug.Log($"Engrenage {gear.gearId} plac� correctement dans l'emplacement {slot.slotId} !");
        Debug.Log($"Progression : {nombreEngrenagesPlaces}/4 engrenages plac�s");

        if (nombreEngrenagesPlaces >= engrenagesMobiles.Length)
        {
            PuzzleComplete();
        }
    }

    private void PuzzleComplete()
    {
        puzzleComplete = true;
        Debug.Log("F�LICITATIONS ! Tous les engrenages sont correctement plac�s !");
  
    }

   
    public void ReinitialiserPuzzle()
    {
        foreach (MovableGear gear in engrenagesMobiles)
        {
            gear.estBienPlace = false;
        }
        foreach (GearSlot slot in emplacements)
        {
            slot.estRempli = false;
        }
        nombreEngrenagesPlaces = 0;
        puzzleComplete = false;
        Debug.Log("Puzzle r�initialis�");
    }
}
