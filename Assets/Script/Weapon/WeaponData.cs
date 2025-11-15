using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public GameObject weaponPrefab;
    public Sprite weaponIcon;

    [Header("Combat Stats")]
    public float damage = 10f;
    public float fireRate = 0.1f;
    public float range = 100f;
    public bool isAutomatic = true;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int maxReserveAmmo = 120;
    public float reloadTime = 2f;

    [Header("Effects")]
    public GameObject muzzleFlash;
    public GameObject impactEffect;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Special")]
    public bool isHealingTool = false;
    public float healAmount = 25f;
}