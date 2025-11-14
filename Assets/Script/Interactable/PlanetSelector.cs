using UnityEngine;


public class PlanetSelector : MonoBehaviour
{
    // Référence à la caméra principale
    [SerializeField] private Camera mainCamera;
    private CameraSwitcher camSwitch;
    private void Start()
    {
        camSwitch = CameraSwitcher.instance;
    }
    void Update()
    {
        if (camSwitch.GetView()) return;
        // Crée un rayon depuis la position de la souris
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Si l’objet touché a le tag "Planet"
            if (hit.collider.CompareTag("Planet"))
            {
                Debug.Log("Planète détectée : " + hit.collider.name);
                OnPlanetSelected(hit.collider.gameObject);
            }
        }
    }

    void DetectPlanet()
    {
        // Crée un rayon depuis la position de la souris
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // On vérifie si le rayon touche quelque chose
        if (Physics.Raycast(ray, out hit))
        {
            // Si l’objet touché a le tag "Planet"
            if (hit.collider.CompareTag("Planet"))
            {
                Debug.Log("Planète détectée : " + hit.collider.name);
                OnPlanetSelected(hit.collider.gameObject);
            }
        }
    }

    // Fonction appelée quand une planète est sélectionnée
    void OnPlanetSelected(GameObject planet)
    {
        // Ici tu pourras lancer ton menu, animation, chargement, etc.
        Debug.Log("Fonction de sélection de planète appelée !");
    }
}
