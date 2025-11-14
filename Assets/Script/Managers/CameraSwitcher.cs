using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour

{
    private static CameraSwitcher Instance { get; set; }
    public static CameraSwitcher instance => Instance;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera planetCamera;

    [Header("Transition Settings")]
    [SerializeField] private float blendTime = 1.5f;

    [Header("Player Control")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInput playerInput;

    private bool isPlanetView = false;
    private CinemachineBrain brain;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }


    void Start()
    {
        // Récupère le Brain sur la caméra principale
        brain = Camera.main.GetComponent<CinemachineBrain>();

        if (brain != null)
        {
            brain.DefaultBlend.Time = blendTime;
        }

        // Active la caméra joueur au départ
        SetPlayerView();
    }

    void Update()
    {
        // Test avec Tab pour switcher (tu peux changer la touche)
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleView();
        }
    }

    public void ToggleView()
    {
        if (isPlanetView)
        {
            SetPlayerView();
        }
        else
        {
            SetPlanetView();
        }
    }

    public void SetPlayerView()
    {
        isPlanetView = false;

        // Change les priorités
        if (playerCamera != null)
            playerCamera.Priority.Value = 10;

        if (planetCamera != null)
            planetCamera.Priority.Value = 5;

        // Réactive les contrôles du joueur
        if (playerController != null)
            playerController.enabled = true;

        if (playerInput != null)
            playerInput.enabled = true;

        // Déverrouille le curseur
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Switched to Player View");
        playerController.SetCanControl(true);
    }

    public void SetPlanetView()
    {
        isPlanetView = true;

        // Change les priorités
        if (playerCamera != null)
            playerCamera.Priority.Value = 5;

        if (planetCamera != null)
            planetCamera.Priority.Value = 10;

        // Désactive les contrôles du joueur (optionnel)
        // if (playerController != null)
        //     playerController.enabled = false;

        // Déverrouille le curseur pour l'UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Switched to Planet View");
        playerController.SetCanControl(false);
    }

    // Méthode pour appeler depuis un bouton UI
    public void OnPlanetMapButtonClick()
    {
        SetPlanetView();
    }

    // Méthode pour retourner au jeu
    public void OnClosePlanetMap()
    {
        SetPlayerView();
    }

    public bool GetView() { return !isPlanetView;  }
}