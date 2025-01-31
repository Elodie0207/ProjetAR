using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Pour recharger la scène si nécessaire


public class CutFils : MonoBehaviour
{
    [System.Serializable]
    public class FilConfig
    {
        public string color;
        public Collider buttonCollider;
        public Animator animator1;
        public Animator animator2;
        public GameObject buttonObject;
        public bool isAlreadyClicked = false;
    }

    [Header("Configuration des fils")]
    public FilConfig filVert = new FilConfig { color = "vert" };
    public FilConfig filNoir = new FilConfig { color = "noir" };
    public FilConfig filBleu = new FilConfig { color = "bleu" };
    public FilConfig filRouge = new FilConfig { color = "rouge" };
    public FilConfig filJaune = new FilConfig { color = "jaune" };

    private bool gameOver = false;

    private void Update()
    {
        if (!gameOver && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                CheckAndTriggerFil(hit.collider, filVert);
                CheckAndTriggerFil(hit.collider, filNoir);
                CheckAndTriggerFil(hit.collider, filBleu);
                CheckAndTriggerFil(hit.collider, filRouge);
                CheckAndTriggerFil(hit.collider, filJaune);
            }
        }
    }

    private void CheckAndTriggerFil(Collider hitCollider, FilConfig fil)
    {
        if (hitCollider == fil.buttonCollider && !fil.isAlreadyClicked && !gameOver)
        {
            // Active les animations
            if (fil.animator1 != null)
            {
                fil.animator1.enabled = true;
                fil.animator1.Play("anim_Fils_" + fil.color + ".001", 0);
            }

            if (fil.animator2 != null)
            {
                fil.animator2.enabled = true;
                fil.animator2.Play("anim_Fils_" + fil.color + ".002", 0);
            }

            // Désactive le bouton et détruit l'objet
            if (fil.buttonCollider != null)
            {
                Destroy(fil.buttonCollider.gameObject);
            }

            if (fil.buttonObject != null)
            {
                Destroy(fil.buttonObject);
            }

            fil.isAlreadyClicked = true;
            Debug.Log("Le fil " + fil.color + " a été coupé !");

            CheckGameCondition(fil.color);
        }
    }

    private void CheckGameCondition(string color)
    {
        switch (color.ToLower())
        {
            case "noir":
                GameOver(false);
                Debug.Log("Vous avez perdu ! Le fil noir était piégé !");
                break;

            case "bleu":
                GameOver(true);
                Debug.Log("Vous avez gagné ! C'était le bon fil !");
                break;
        }
    }

    private void GameOver(bool hasWon)
    {
        gameOver = true;
        if (hasWon)
        {
            Debug.Log("Félicitations ! Vous avez désamorcé la bombe !");
        }
        else
        {
            Debug.Log("BOOM ! La bombe a explosé !");
        }

        // Retirer la recharge automatique de la scène
        // Invoke("ReloadScene", 3f);
    }

    // On ne recharge plus automatiquement la scène
    /*private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/
}