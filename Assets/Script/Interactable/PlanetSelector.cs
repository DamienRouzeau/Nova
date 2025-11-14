using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlanetSelector : MonoBehaviour
{
    private static PlanetSelector Instance { get; set; }
    public static PlanetSelector instance => Instance;

    // Référence à la caméra principale
    [SerializeField] private Camera mainCamera;
    private CameraSwitcher camSwitch;
    [SerializeField] private List<PlanetBehaviour> planets = new List<PlanetBehaviour>();
    private PlanetBehaviour currentHoveredPlanet;
    private PlanetBehaviour currentSelectedPlanet;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        camSwitch = CameraSwitcher.instance;
    }
    void Update()
    {
        if (!camSwitch.GetView())
        { 
            DetectHover();
            DetectClick();
        }
    }

    void DetectHover()
    {
        if (currentSelectedPlanet) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Planet"))
            {
                PlanetBehaviour ph = hit.collider.GetComponent<PlanetBehaviour>();
                Debug.Log("Planet detected : " + hit.collider.gameObject.name);

                if (ph != currentHoveredPlanet)
                {
                    if (currentHoveredPlanet != null)
                        currentHoveredPlanet.SetHover(false);

                    currentHoveredPlanet = ph;
                    currentHoveredPlanet.SetHover(true);
                }
                return;
            }
        }

        // Si on ne survole plus rien
        if (currentHoveredPlanet != null)
        {
            currentHoveredPlanet.SetHover(false);
            currentHoveredPlanet = null;
        }
    }

    void DetectClick()
    {
        if (currentSelectedPlanet) return;
        // Si le joueur clique
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Planet"))
                {
                    currentSelectedPlanet = hit.collider.GetComponent<PlanetBehaviour>();
                    currentHoveredPlanet.SetHover(false);
                    OnPlanetSelected(currentSelectedPlanet);
                }
            }
        }
    }

    // --------------------------------------
    void OnPlanetSelected(PlanetBehaviour planet)
    {
        planet.SelectPlanet();
    }

    public void DeselectPlanet()
    {
        currentSelectedPlanet.DisableUI();
        currentSelectedPlanet = null;
    }
}
