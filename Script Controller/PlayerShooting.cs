// PlayerShooting.cs — Updated: ForceKnifeOnly() untuk Sudden Death
// Perubahan:
//   - Tambah bool isKnifeOnlyMode: saat true, tombol WeaponSwitch diblokir,
//     pemain selalu dalam melee mode.
//   - Tambah metode publik ForceKnifeOnly() dipanggil GameManager saat Sudden Death.
//   - ResetToPrimary() juga mereset isKnifeOnlyMode = false.
// File ini menggantikan: Script Controller/PlayerShooting.cs

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;

    [Header("Melee References")]
    [Tooltip("GameObject pisau/melee yang menempel ke player (child object)")]
    public GameObject meleeObject;
    [Tooltip("SpriteRenderer dari meleeObject")]
    public SpriteRenderer meleeSpriteRenderer;

    [Header("Runtime (diisi oleh GameInitializer)")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate    = 0.5f;

    [HideInInspector] public WeaponData     weaponData;
    [HideInInspector] public SpriteRenderer weaponSpriteRenderer;

    // Events untuk AmmoUI
    public System.Action<int> OnAmmoPrimaryChanged;
    public System.Action      OnSwitchedToSecondary;
    public System.Action<int> OnResetAmmo;
    public System.Action      OnSwitchedToMelee;
    public System.Action      OnSwitchedFromMelee;

    // ── State ─────────────────────────────────────────────────────────────────
    private int   currentAmmo      = 0;
    private bool  isUsingSecondary = false;
    private bool  isPrimaryEmpty   = false;
    private bool  isFacingRight    = true;
    private float nextFireTime     = 0f;
    private bool  isMeleeMode      = false;
    private float nextMeleeTime    = 0f;
    private bool  isKnifeOnlyMode  = false;   // ← NEW: Sudden Death lock
    private AudioManager audioManager;

    // ── Melee Animation ───────────────────────────────────────────────────────
    private readonly Quaternion meleeRotIdle   = Quaternion.Euler(0f, 0f,  45f);
    private readonly Quaternion meleeRotAttack = Quaternion.Euler(0f, 0f, -20f);
    private readonly Vector3    meleeLocalPosIdle = new Vector3(0.75f, -0.1f, 0f);

    private bool  meleeAnimating  = false;
    private float meleeAnimTimer  = 0f;
    private const float MELEE_ANIM_DURATION = 0.15f;

    // ── Device guard ──────────────────────────────────────────────────────────
    private PlayerInput m_PlayerInput;
    private string      m_AssetName;
    private InputAction m_ShootAction;
    private InputAction m_WeaponSwitchAction;

    private void Awake()
    {
        audioManager  = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        m_PlayerInput = GetComponent<PlayerInput>();
        m_AssetName   = m_PlayerInput?.actions?.name ?? string.Empty;

        if (meleeObject != null) meleeObject.SetActive(false);
    }

    private void Start()
    {
        m_ShootAction        = m_PlayerInput?.actions?.FindAction("Shoot");
        m_WeaponSwitchAction = m_PlayerInput?.actions?.FindAction("WeaponSwitch");

        if (meleeObject != null)
        {
            meleeObject.transform.localPosition = meleeLocalPosIdle;
            meleeObject.transform.localRotation = meleeRotIdle;
            meleeObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (meleeAnimating)
        {
            meleeAnimTimer += Time.deltaTime;
            if (meleeAnimTimer >= MELEE_ANIM_DURATION)
            {
                meleeAnimating = false;
                meleeAnimTimer = 0f;
                if (meleeObject != null)
                    meleeObject.transform.localRotation = meleeRotIdle;
            }
        }
    }

    public void InitAmmo()
    {
        if (weaponData == null) return;

        isUsingSecondary = false;
        isPrimaryEmpty   = false;
        isMeleeMode      = false;
        currentAmmo      = weaponData.maxAmmo;
        bulletPrefab     = weaponData.bulletPrefab;
        bulletSpeed      = weaponData.bulletSpeed;
        fireRate         = weaponData.fireRate;

        OnAmmoPrimaryChanged?.Invoke(currentAmmo);

        if (meleeSpriteRenderer != null && weaponData.meleeSprite != null)
            meleeSpriteRenderer.sprite = weaponData.meleeSprite;

        if (meleeObject != null) meleeObject.SetActive(false);
        if (weaponSpriteRenderer != null) weaponSpriteRenderer.gameObject.SetActive(true);
    }

    public void SetFacingDirection(bool facingRight)
    {
        isFacingRight = facingRight;
    }

    // ── INPUT: Shoot / Melee attack ───────────────────────────────────────────
    public void OnShoot(InputValue value)
    {
        if (Time.timeScale == 0f) return;

        var activeDevice = m_ShootAction?.activeControl?.device;
        if (activeDevice is Gamepad g && !ControllerAssignmentManager.IsDeviceAllowedFor(m_AssetName, g))
            return;

        if (!value.isPressed) return;

        if (isMeleeMode) { PerformMeleeAttack(); return; }
        if (isKnifeOnlyMode) { PerformMeleeAttack(); return; }  // Sudden Death: selalu melee
        if (Time.time < nextFireTime) return;

        if (isPrimaryEmpty)
        {
            PlaySound(weaponData?.emptySound);
            nextFireTime = Time.time + fireRate;
            return;
        }

        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    // ── INPUT: WeaponSwitch ───────────────────────────────────────────────────
    public void OnWeaponSwitch(InputValue value)
    {
        if (Time.timeScale == 0f) return;

        // Blokir weapon switch saat Sudden Death
        if (isKnifeOnlyMode) return;

        var activeDevice = m_WeaponSwitchAction?.activeControl?.device;
        if (activeDevice is Gamepad g && !ControllerAssignmentManager.IsDeviceAllowedFor(m_AssetName, g))
            return;

        if (!value.isPressed) return;
        ToggleWeaponMode();
    }

    // ── Toggle primary ↔ melee ────────────────────────────────────────────────
    private void ToggleWeaponMode()
    {
        if (isMeleeMode)
        {
            isMeleeMode = false;
            if (meleeObject != null) meleeObject.SetActive(false);
            if (weaponSpriteRenderer != null)
            {
                weaponSpriteRenderer.gameObject.SetActive(true);
                if (isUsingSecondary && weaponData?.secondarySprite != null)
                    weaponSpriteRenderer.sprite = weaponData.secondarySprite;
                else if (weaponData?.weaponSprite != null)
                    weaponSpriteRenderer.sprite = weaponData.weaponSprite;
            }
            PlaySound(weaponData?.weaponSwitchSound);
            OnSwitchedFromMelee?.Invoke();
        }
        else
        {
            isMeleeMode = true;
            if (weaponSpriteRenderer != null) weaponSpriteRenderer.gameObject.SetActive(false);
            if (meleeObject != null)
            {
                meleeObject.transform.localPosition = meleeLocalPosIdle;
                meleeObject.transform.localRotation = meleeRotIdle;
                meleeObject.SetActive(true);
                if (meleeSpriteRenderer != null && weaponData?.meleeSprite != null)
                    meleeSpriteRenderer.sprite = weaponData.meleeSprite;
            }
            PlaySound(weaponData?.weaponSwitchSound);
            OnSwitchedToMelee?.Invoke();
        }
    }

    // ── Serangan Melee ────────────────────────────────────────────────────────
    private void PerformMeleeAttack()
    {
        if (weaponData == null) return;
        if (Time.time < nextMeleeTime) return;

        nextMeleeTime = Time.time + weaponData.meleeFireRate;
        PlaySound(weaponData.meleeSound);

        if (meleeObject != null)
        {
            meleeObject.transform.localRotation = meleeRotAttack;
            meleeAnimating = true;
            meleeAnimTimer = 0f;
        }

        Vector2 hitCenter = (Vector2)transform.position +
                            (isFacingRight ? Vector2.right : Vector2.left) * (weaponData.meleeRange * 0.5f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, weaponData.meleeRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            // Coba semua komponen Health (P1, P2, P3, P4)
            Health h1 = hit.GetComponent<Health>();
            if (h1 != null) { h1.TakeDamage(weaponData.meleeDamage); continue; }

            HealthP2 h2 = hit.GetComponent<HealthP2>();
            if (h2 != null) { h2.TakeDamage(weaponData.meleeDamage); continue; }

            HealthP3 h3 = hit.GetComponent<HealthP3>();
            if (h3 != null) { h3.TakeDamage(weaponData.meleeDamage); continue; }

            HealthP4 h4 = hit.GetComponent<HealthP4>();
            if (h4 != null) { h4.TakeDamage(weaponData.meleeDamage); continue; }
        }
    }

    // ── Shoot (peluru) ────────────────────────────────────────────────────────
    private void Shoot()
    {
        if (Time.timeScale == 0f) return;
        if (bulletPrefab == null) { Debug.LogWarning("bulletPrefab belum diset!"); return; }

        bool wasUsingSecondary    = isUsingSecondary;
        bool shouldSwitchAfterShot = false;

        if (!isUsingSecondary)
        {
            currentAmmo--;
            currentAmmo = Mathf.Max(0, currentAmmo);
            OnAmmoPrimaryChanged?.Invoke(currentAmmo);

            if (currentAmmo <= 0)
            {
                if (weaponData != null && weaponData.hasSecondary && weaponData.secondaryBulletPrefab != null)
                    shouldSwitchAfterShot = true;
                else
                    isPrimaryEmpty = true;
            }
        }

        GameObject spawnPrefab = wasUsingSecondary ? weaponData.secondaryBulletPrefab : bulletPrefab;
        float spawnSpeed  = wasUsingSecondary ? weaponData.secondaryBulletSpeed : bulletSpeed;
        float spawnDamage = wasUsingSecondary ? weaponData.secondaryDamage      : weaponData.damage;

        GameObject bullet = Instantiate(spawnPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb    = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = isFacingRight ? Vector2.right * spawnSpeed : Vector2.left * spawnSpeed;

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null) bulletScript.damage = spawnDamage;

        Destroy(bullet, 2f);

        if (wasUsingSecondary) PlaySound(weaponData?.secondaryShootSound);
        else                   PlaySound(weaponData?.shootSound);

        if (shouldSwitchAfterShot) SwitchToSecondary();
    }

    private void SwitchToSecondary()
    {
        isUsingSecondary = true;
        bulletPrefab     = weaponData.secondaryBulletPrefab;
        bulletSpeed      = weaponData.secondaryBulletSpeed;
        fireRate         = weaponData.secondaryFireRate;

        if (weaponSpriteRenderer != null && weaponData.secondarySprite != null)
            weaponSpriteRenderer.sprite = weaponData.secondarySprite;

        PlaySound(weaponData?.weaponSwitchSound);
        OnSwitchedToSecondary?.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioManager?.PlaySFX(clip);
    }

    public void ResetToPrimary()
    {
        isKnifeOnlyMode = false;   // Reset mode Sudden Death

        if (weaponData == null) return;

        if (isMeleeMode)
        {
            isMeleeMode = false;
            if (meleeObject != null) meleeObject.SetActive(false);
            if (weaponSpriteRenderer != null) weaponSpriteRenderer.gameObject.SetActive(true);
        }

        InitAmmo();

        if (weaponSpriteRenderer != null && weaponData.weaponSprite != null)
            weaponSpriteRenderer.sprite = weaponData.weaponSprite;

        OnResetAmmo?.Invoke(weaponData.maxAmmo);
    }

    // ── ForceKnifeOnly (Sudden Death) ─────────────────────────────────────────
    // Paksa pemain ke melee mode dan blokir weapon switch.
    public void ForceKnifeOnly()
    {
        isKnifeOnlyMode = true;

        // Jika belum dalam melee mode, paksa masuk
        if (!isMeleeMode)
        {
            isMeleeMode = true;
            if (weaponSpriteRenderer != null) weaponSpriteRenderer.gameObject.SetActive(false);
            if (meleeObject != null)
            {
                meleeObject.transform.localPosition = meleeLocalPosIdle;
                meleeObject.transform.localRotation = meleeRotIdle;
                meleeObject.SetActive(true);
                if (meleeSpriteRenderer != null && weaponData?.meleeSprite != null)
                    meleeSpriteRenderer.sprite = weaponData.meleeSprite;
            }
            OnSwitchedToMelee?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponData == null) return;
        Vector2 hitCenter = (Vector2)transform.position +
                            (isFacingRight ? Vector2.right : Vector2.left) * (weaponData.meleeRange * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, weaponData.meleeRange);
    }
}