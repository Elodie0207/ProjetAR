using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deviser : MonoBehaviour
{
    [System.Serializable]
    public class VisConfig
    {
        public string nom;
        public BoxCollider zoneVissage;
        public bool estDevissee = false;
    }

    [Header("Configuration des vis")]
    public VisConfig visHautGauche = new VisConfig { nom = "Vis Haut Gauche" };
    public VisConfig visHautDroite = new VisConfig { nom = "Vis Haut Droite" };
    public VisConfig visBasGauche = new VisConfig { nom = "Vis Bas Gauche" };
    public VisConfig visBasDroite = new VisConfig { nom = "Vis Bas Droite" };

    [Header("Configuration Tournevis")]
    public Transform pointTournevis;

    private int nombreVisDevissees = 0;

    private void Update()
    {
        VerifierProximiteTournevis(visHautGauche);
        VerifierProximiteTournevis(visHautDroite);
        VerifierProximiteTournevis(visBasGauche);
        VerifierProximiteTournevis(visBasDroite);
    }

    private void VerifierProximiteTournevis(VisConfig vis)
    {
        if (vis.estDevissee || vis.zoneVissage == null || pointTournevis == null)
            return;

        if (vis.zoneVissage.bounds.Contains(pointTournevis.position))
        {
            vis.estDevissee = true;
            nombreVisDevissees++;
            Debug.Log(vis.nom + " dévissée !");

            if (nombreVisDevissees >= 4)
            {
                Debug.Log("Toutes les vis sont dévissées !");
            }
        }
    }
}
