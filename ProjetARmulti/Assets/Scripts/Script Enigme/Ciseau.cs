using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ciseau : MonoBehaviour
{
    [System.Serializable]
    public class FilConfig
    {
        public string color;
        public GameObject filPartieGauche;  // Première partie du fil
        public GameObject filPartieDroite;  // Deuxième partie du fil
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
    public Transform pointCiseau;       // Point de référence sur l'Image Target du ciseau
    public float distanceCoupe = 0.1f;  // Distance de détection pour la coupe

    private bool partieTerminee = false;

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
                FinPartie(false);
                Debug.Log("Vous avez perdu ! Le fil noir était piégé !");
                break;
            case "bleu":
                FinPartie(true);
                Debug.Log("Vous avez gagné ! C'était le bon fil !");
                break;
            default:
                Debug.Log("Ce fil n'était pas le bon, continuez à chercher !");
                break;
        }
    }

    private void FinPartie(bool victoire)
    {
        partieTerminee = true;
        if (victoire)
        {
            Debug.Log("Félicitations ! Vous avez désamorcé la bombe !");
            // Ajoutez ici le code pour la victoire (effets, sons, etc.)
        }
        else
        {
            Debug.Log("BOOM ! La bombe a explosé !");
            // Ajoutez ici le code pour l'échec (effets, sons, etc.)
        }
    }
}
