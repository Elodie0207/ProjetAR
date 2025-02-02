using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Buttonsync : MonoBehaviour
{
    [Header("Boutons")]
    public BoxCollider boutonJoueur1;
    public BoxCollider boutonJoueur2;

    [Header("Configuration")]
    public float delaiMaxEntreAppuis = 1.0f;

    [Header("Feedback")]
    public GameObject objetAActiver;

    private bool joueur1Appuye = false;
    private bool joueur2Appuye = false;
    private float tempsAppuiJoueur1 = 0f;
    private float tempsAppuiJoueur2 = 0f;

    private void Update()
    {
        // Pour les tests PC avec souris
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == boutonJoueur1)
                {
                    OnJoueur1Click();
                }
                else if (hit.collider == boutonJoueur2)
                {
                    OnJoueur2Click();
                }
            }
        }

        // Pour mobile (garder la partie tactile)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == boutonJoueur1)
                {
                    OnJoueur1Click();
                }
                else if (hit.collider == boutonJoueur2)
                {
                    OnJoueur2Click();
                }
            }
        }
    }

    private void OnJoueur1Click()
    {
        joueur1Appuye = true;
        tempsAppuiJoueur1 = Time.time;
        Debug.Log("Joueur 1 a appuy� ! En attente du Joueur 2...");
        VerifierSynchronisation();

        Invoke("ResetJoueur1", delaiMaxEntreAppuis);
    }

    private void OnJoueur2Click()
    {
        joueur2Appuye = true;
        tempsAppuiJoueur2 = Time.time;
        Debug.Log("Joueur 2 a appuy� ! En attente du Joueur 1...");
        VerifierSynchronisation();

        Invoke("ResetJoueur2", delaiMaxEntreAppuis);
    }

    private void VerifierSynchronisation()
    {
        if (joueur1Appuye && joueur2Appuye)
        {
            float diffTemps = Mathf.Abs(tempsAppuiJoueur1 - tempsAppuiJoueur2);

            if (diffTemps <= delaiMaxEntreAppuis)
            {
                Debug.Log("SUCC�S ! Les deux joueurs ont appuy� en m�me temps !");
                ActionSynchronisee();
            }
            else
            {
                Debug.Log("�CHEC ! L'appui n'�tait pas synchronis� - Diff�rence : " + diffTemps.ToString("F2") + " secondes");
                ResetTout();
            }
        }
    }

    private void ActionSynchronisee()
    {
        if (objetAActiver != null)
        {
            objetAActiver.SetActive(true);
            Debug.Log("Objet activ� avec succ�s !");
        }
        ResetTout();
    }

    private void ResetJoueur1()
    {
        if (joueur1Appuye)
        {
            joueur1Appuye = false;
            Debug.Log("Temps �coul� pour Joueur 1 - R�essayez !");
        }
    }

    private void ResetJoueur2()
    {
        if (joueur2Appuye)
        {
            joueur2Appuye = false;
            Debug.Log("Temps �coul� pour Joueur 2 - R�essayez !");
        }
    }

    private void ResetTout()
    {
        joueur1Appuye = false;
        joueur2Appuye = false;
        CancelInvoke("ResetJoueur1");
        CancelInvoke("ResetJoueur2");
    }
}
