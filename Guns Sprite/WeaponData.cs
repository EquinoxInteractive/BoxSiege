using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Primary Weapon")]
    public string weaponName;
    public Sprite weaponSprite;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate = 0.5f;
    public float damage = 10f;
    public int maxAmmo = 10;
    public AudioClip shootSound;       // Suara tembak normal (ammo masih ada)
    public AudioClip emptySound;       // Suara ketika ammo habis & tidak ada secondary

    [Header("Secondary Weapon (Optional)")]
    public bool hasSecondary = false;
    public Sprite secondarySprite;
    public GameObject secondaryBulletPrefab;
    public float secondaryBulletSpeed = 8f;
    public float secondaryFireRate = 0.8f;
    public float secondaryDamage = 5f;
    public AudioClip secondaryShootSound; // Suara tembak secondary (unlimited)

    [Header("Melee Weapon")]
    public Sprite meleeSprite;            // Sprite pisau/melee (assign di inspector)
    public float meleeDamage = 25f;       // Damage melee
    public float meleeFireRate = 0.4f;    // Cooldown antara setiap serangan melee
    public float meleeRange = 1.2f;       // Jangkauan melee (jarak deteksi musuh)
    public AudioClip meleeSound;          // Suara ketika melee menyerang

    [Header("Weapon Switch")]
    [Tooltip("SFX yang dimainkan setiap kali pemain mengganti senjata (Primary ↔ Melee, atau auto-switch ke Secondary)")]
    public AudioClip weaponSwitchSound;   // Suara ketika mengganti senjata apapun
}