using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ciseau : MonoBehaviour
{
    [System.Serializable]
    public class FilConfig
    {
        public string color;
        public GameObject filPartieGauche;  // Partie gauche du fil
        public GameObject filPartieDroite;  // Partie droite du fil
        public Animator animatorGauche;     // Animation partie gauche
        public Animator animatorDroite;     // Animation partie droite
        public BoxCollider zoneCoupe;       // Zone où le ciseau peut couper
        public bool estCoupe = false;
    }

    [Header("Configuration des fils")]
    public FilConfig filVert = new FilConfig { color = "vert" };
    public FilConfig filNoir = new FilConfig { color = "noir" };
    public FilConfig filBleu = new FilConfig { color = "bleu" };
    public FilConfig filRouge = new FilConfig { color = "rouge" };
    public FilConfig filJaune = new FilConfig { color = "jaune" };

    [Header("Configuration Ciseau")]
    public Transform pointCiseau;       // Position du ciseau
    public float distanceCoupe = 0.1f;  // Distance de détection pour la coupe

    private bool partieTerminee = false;

    // Référence au GameManager
    public GameManager gameManager; // À lier dans l'inspecteur

    private void Update()
    {
        if (!partieTerminee)
        {
            VerifierProximiteCiseau(filVert);
            VerifierProximiteCiseau(filNoir);
            VerifierProximiteCiseau(filBleu);
            VerifierProximiteCiseau(filRouge);
            VerifierProximiteCiseau(filJaune);
        }
    }

    private void VerifierProximiteCiseau(FilConfig fil)
    {
        if (fil.estCoupe || fil.zoneCoupe == null || pointCiseau == null)
            return;

        // Vérifie si le ciseau est dans la zone de coupe du fil
        if (fil.zoneCoupe.bounds.Contains(pointCiseau.position))
        {
            CouperFil(fil);
        }
    }

    private void CouperFil(FilConfig fil)
    {
        if (!fil.estCoupe)
        {
            // Active les animations des deux parties
            if (fil.animatorGauche != null)
            {
                fil.animatorGauche.enabled = true;
                fil.animatorGauche.Play("anim_Fils_" + fil.color + "_gauche");
            }

            if (fil.animatorDroite != null)
            {
                fil.animatorDroite.enabled = true;
                fil.animatorDroite.Play("anim_Fils_" + fil.color + "_droite");
            }

            fil.estCoupe = true;
            Debug.Log("Le fil " + fil.color + " a été coupé !");
            VerifierConditionJeu(fil.color);
        }
    }

    private void VerifierConditionJeu(string color)
    {
        switch (color.ToLower())
        {
            case "noir":
                Debug.Log("Vous avez perdu ! Le fil noir était piégé !");
                FinPartie(false, true); // Chrono à zéro
                break;
            case "bleu":
                Debug.Log("Vous avez gagné ! C'était le bon fil !");
                FinPartie(true, false); // Victoire
                gameManager.SetLightColor(true);
                break;
            default:
                Debug.Log("Ce fil n'était pas le bon, continuez à chercher !");
                FinPartie(false, false); // Perte de temps seulement
                break;
        }
    }

    private void FinPartie(bool victoire, bool chronoZero)
    {
        partieTerminee = true;

        if (gameManager != null)
        {
            if (victoire)
            {
                Debug.Log("Félicitations ! Vous avez désamorcé la bombe !");
            }
            else
            {
                if (chronoZero)
                {
                    gameManager.ReduceTime(gameManager.timeRemaining); // Met le chrono à 0
                    Debug.Log("BOOM ! Le fil noir était piégé, le chrono est à zéro !");
                }
                else
                {
                    gameManager.ReduceTime(120f); // Retire 2 minutes du chrono
                    Debug.Log("Mauvais fil coupé, 2 minutes en moins !");
                }
            }
        }
    }
}
