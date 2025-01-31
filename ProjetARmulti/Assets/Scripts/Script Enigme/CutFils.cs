using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Pour recharger la sc�ne si n�cessaire


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

            // D�sactive le bouton et d�truit l'objet
            if (fil.buttonCollider != null)
            {
                Destroy(fil.buttonCollider.gameObject);
            }

            if (fil.buttonObject != null)
            {
                Destroy(fil.buttonObject);
            }

            fil.isAlreadyClicked = true;
            Debug.Log("Le fil " + fil.color + " a �t� coup� !");

            CheckGameCondition(fil.color);
        }
    }

    private void CheckGameCondition(string color)
    {
        switch (color.ToLower())
        {
            case "noir":
                GameOver(false);
                Debug.Log("Vous avez perdu ! Le fil noir �tait pi�g� !");
                break;

            case "bleu":
                GameOver(true);
                Debug.Log("Vous avez gagn� ! C'�tait le bon fil !");
                break;
        }
    }

    private void GameOver(bool hasWon)
    {
        gameOver = true;
        if (hasWon)
        {
            Debug.Log("F�licitations ! Vous avez d�samorc� la bombe !");
        }
        else
        {
            Debug.Log("BOOM ! La bombe a explos� !");
        }

        // Retirer la recharge automatique de la sc�ne
        // Invoke("ReloadScene", 3f);
    }

    // On ne recharge plus automatiquement la sc�ne
    /*private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/
}