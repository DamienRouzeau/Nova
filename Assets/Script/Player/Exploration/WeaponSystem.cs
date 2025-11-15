using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private int currentWeaponIndex = 0;
    private WeaponData currentWeapon;
    private GameObject currentWeaponObject;

    private float nextFireTime;
    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;
    private bool isFiring;
    private bool isAiming;

    void Start()
    {
        if (availableWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        if (currentWeapon == null) return;

        // Tir automatique
        if (isFiring && !isReloading && currentWeapon.isAutomatic)
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
            }
        }
    }

    public void StartFiring()
    {
        if (currentWeapon == null || isReloading) return;

        isFiring = true;

        // Tir semi-automatique
        if (!currentWeapon.isAutomatic && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    void Fire()
    {
        if (currentAmmo <= 0)
        {
            // Son de clic vide
            if (audioSource != null && currentWeapon.emptySound != null)
            {
                audioSource.PlayOneShot(currentWeapon.emptySound);
            }
            return;
        }

        // Consomme munition
        currentAmmo--;
        nextFireTime = Time.time + currentWeapon.fireRate;

        // Son de tir
        if (audioSource != null && currentWeapon.fireSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.fireSound);
        }

        // Raycast pour détecter impact
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, currentWeapon.range))
        {
            // Dégâts
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(currentWeapon.damage);
            }

            // Impact effect
            if (currentWeapon.impactEffect != null)
            {
                Instantiate(currentWeapon.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        // Muzzle flash
        if (currentWeapon.muzzleFlash != null && shootPoint != null)
        {
            Instantiate(currentWeapon.muzzleFlash, shootPoint.position, shootPoint.rotation);
        }

        // Recul (à implémenter avec animation)
        // ApplyRecoil();
    }

    public void Reload()
    {
        if (isReloading || currentAmmo == currentWeapon.magazineSize || reserveAmmo <= 0)
            return;

        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        isFiring = false;

        if (audioSource != null && currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        int ammoNeeded = currentWeapon.magazineSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        isReloading = false;
    }

    public void SwitchToNextWeapon()
    {
        if (availableWeapons.Count <= 1) return;

        currentWeaponIndex = (currentWeaponIndex + 1) % availableWeapons.Count;
        EquipWeapon(currentWeaponIndex);
    }

    public void SwitchToPreviousWeapon()
    {
        if (availableWeapons.Count <= 1) return;

        currentWeaponIndex--;
        if (currentWeaponIndex < 0)
            currentWeaponIndex = availableWeapons.Count - 1;

        EquipWeapon(currentWeaponIndex);
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= availableWeapons.Count) return;

        // Détruit l'arme actuelle
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }

        currentWeapon = availableWeapons[index];

        // Instancie nouvelle arme
        if (currentWeapon.weaponPrefab != null && weaponHolder != null)
        {
            currentWeaponObject = Instantiate(currentWeapon.weaponPrefab, weaponHolder);
            currentWeaponObject.transform.localPosition = Vector3.zero;
            currentWeaponObject.transform.localRotation = Quaternion.identity;
        }

        // Initialise munitions
        currentAmmo = currentWeapon.magazineSize;
        reserveAmmo = currentWeapon.maxReserveAmmo;

        isReloading = false;
        isFiring = false;
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
        // Animation de visée à implémenter
    }

    public void AddAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(reserveAmmo + amount, currentWeapon.maxReserveAmmo);
    }

    // Getters
    public int GetCurrentAmmo() => currentAmmo;
    public int GetReserveAmmo() => reserveAmmo;
    public bool IsReloading() => isReloading;
    public WeaponData GetCurrentWeapon() => currentWeapon;
}

// Interface pour objets pouvant prendre des dégâts
public interface IDamageable
{
    void TakeDamage(float damage);
}