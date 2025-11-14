using UnityEngine;
using Unity.Cinemachine;
using System.Collections;


public class PlanetBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject hover;
    [SerializeField] CinemachineCamera camera;
    [SerializeField] GameObject uiInfo;

    public void SetHover(bool isHovered)
    {
        hover.SetActive(isHovered);
    }

    public void SelectPlanet()
    {
        CameraSwitcher.instance.PlanetSelected(this);
        StartCoroutine(UIDelayed());
    }

    private IEnumerator UIDelayed()
    {
        yield return new WaitForSeconds(1.2f);
        uiInfo.SetActive(true);
    }

    public void DisableUI()
    {
        uiInfo.SetActive(false);
    }

    public void UnselectPlanet()
    {
        camera.Priority.Value = 5;
    }

    public CinemachineCamera GetCamera() { return camera; }
}
