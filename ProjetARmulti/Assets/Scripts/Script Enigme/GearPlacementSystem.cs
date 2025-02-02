using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearPlacementSystem : MonoBehaviour
{
    [Header("Configuration Position 1")]
    public Transform position1;
    public GameObject engrenage6;        // Engrenage sur target principale
    public Transform engrenage6Target;   // La target mobile de l'engrenage6

    [Header("Configuration Position 4")]
    public Transform position4;
    public GameObject engrenage2;        // Engrenage sur target principale
    public Transform engrenage2Target;   // La target mobile de l'engrenage2

    [Header("Paramètres de verrouillage")]
    public float distanceValidation = 1f;
    private bool engrenage6Verrouille = false;
    private bool engrenage2Verrouille = false;

    void Start()
    {
        // Désactive les engrenages sur la target principale au démarrage
        if (engrenage6 != null) engrenage6.SetActive(false);
        if (engrenage2 != null) engrenage2.SetActive(false);

        Debug.Log("Script initialisé - Engrenages désactivés");
    }

    void Update()
    {
        // Vérification pour l'engrenage 6
        if (!engrenage6Verrouille && engrenage6Target != null)
        {
            float distance = Vector3.Distance(engrenage6Target.position, position1.position);
            Debug.Log("Distance engrenage6: " + distance);
            if (distance < distanceValidation)
            {
                VerrouillerEngrenage6();
            }
        }

        // Vérification pour l'engrenage 2
        if (!engrenage2Verrouille && engrenage2Target != null)
        {
            float distance = Vector3.Distance(engrenage2Target.position, position4.position);
            Debug.Log("Distance engrenage2: " + distance);
            if (distance < distanceValidation)
            {
                VerrouillerEngrenage2();
            }
        }
    }

    void VerrouillerEngrenage6()
    {
        engrenage6Verrouille = true;
        engrenage6.SetActive(true);
        Debug.Log("Engrenage 6 verrouillé!");
        // Désactive le modèle sur la target
        if (engrenage6Target != null)
        {
            var modeleSurTarget = engrenage6Target.GetComponentInChildren<MeshRenderer>();
            if (modeleSurTarget != null) modeleSurTarget.enabled = false;
        }
    }

    void VerrouillerEngrenage2()
    {
        engrenage2Verrouille = true;
        engrenage2.SetActive(true);
        Debug.Log("Engrenage 2 verrouillé!");
        // Désactive le modèle sur la target
        if (engrenage2Target != null)
        {
            var modeleSurTarget = engrenage2Target.GetComponentInChildren<MeshRenderer>();
            if (modeleSurTarget != null) modeleSurTarget.enabled = false;
        }
    }
}