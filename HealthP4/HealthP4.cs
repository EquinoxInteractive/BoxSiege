// HealthP4.cs — Updated: isEliminated flag
// File ini menggantikan: HealthP4/HealthP4.cs

using UnityEngine;
using UnityEngine.InputSystem;

public class HealthP4 : MonoBehaviour
{
    [SerializeField] private float startingHealth = 3f;
    public float currentHealth { get; private set; }

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayeJump2D    jumpScript;
    [SerializeField] private MonoBehaviour  shotScript;

    private SpriteRenderer spriteRenderer;
    private bool isDead       = false;
    private bool isEliminated = false;
    private int  lives        = 3;

    AudioManager audioManager;
    public GameManager gameManager;
    private PlayerPowerUpEffects powerUpEffects;

    private void Awake()
    {
        audioManager   = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();
        currentHealth  = startingHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        powerUpEffects = GetComponent<PlayerPowerUpEffects>();

        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (jumpScript     == null) jumpScript     = GetComponent<PlayeJump2D>();
        if (gameManager    == null) gameManager    = FindObjectOfType<GameManager>();

        if (GameData.Instance?.player4Character != null)
            spriteRenderer.sprite = GameData.Instance.player4Character.aliveSprite;
    }

    public void TakeDamage(float _damage)
    {
        if (isEliminated) return;
        if (_damage <= 0)
        {
            currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);
            return;
        }

        float remainingDamage = _damage;
        if (powerUpEffects != null)
        {
            int absorbed = powerUpEffects.AbsorbWithShield(_damage);
            if (absorbed > 0)
            {
                if (audioManager != null) audioManager.PlaySFX(audioManager.shieldPowerUp);
                remainingDamage = Mathf.Max(0f, _damage - absorbed);
            }
        }

        if (remainingDamage <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - remainingDamage, 0, startingHealth);

        if (currentHealth <= 0 && !isDead)
        {
            if (audioManager != null) audioManager.PlaySFX(audioManager.hit);
            lives--;
            isDead = true;
            if (lives <= 0)
            {
                DisablePlayer(true);
                gameManager?.ReportPlayerDeath(4);
            }
            else
            {
                DisablePlayer(true);
            }
        }
        else if (currentHealth > 0 && _damage > 0)
        {
            if (audioManager != null) audioManager.PlaySFX(audioManager.hit);
        }
    }

    public void DisablePlayer(bool showDeathSprite)
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (jumpScript     != null) jumpScript.enabled     = false;
        if (shotScript     != null) shotScript.enabled     = false;

        if (showDeathSprite && spriteRenderer != null &&
            GameData.Instance?.player4Character != null)
            spriteRenderer.sprite = GameData.Instance.player4Character.deathSprite;

        var pi = GetComponent<PlayerInput>();
        if (pi != null) pi.enabled = false;
    }

    public void EnablePlayer()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (jumpScript     != null) jumpScript.enabled     = true;
        if (shotScript     != null) shotScript.enabled     = true;

        var pi = GetComponent<PlayerInput>();
        if (pi != null) pi.enabled = true;

        if (spriteRenderer != null && GameData.Instance?.player4Character != null)
            spriteRenderer.sprite = GameData.Instance.player4Character.aliveSprite;

        isDead       = false;
        isEliminated = false;
    }

    public void ResetHealth()
    {
        currentHealth = startingHealth;
        isDead        = false;
        isEliminated  = false;
        lives         = 3;
        if (spriteRenderer != null && GameData.Instance?.player4Character != null)
            spriteRenderer.sprite = GameData.Instance.player4Character.aliveSprite;
    }

    // Dipanggil saat masuk Sudden Death untuk peserta:
    // HP = 1, lives = 1 → 1 kali kena pisau langsung eliminasi.
    // Ini tidak mengubah logika normal round sama sekali.
    public void ResetHealthSuddenDeath()
    {
        currentHealth = 3f;
        lives         = 1;
        isDead        = false;
        isEliminated  = false;
        if (spriteRenderer != null && GameData.Instance?.player4Character != null)
            spriteRenderer.sprite = GameData.Instance.player4Character.aliveSprite;
    }

    public void SetHealthToZero()
    {
        currentHealth = 0f;
        lives         = 0;
        isDead        = true;
        isEliminated  = true;
    }

    public bool IsOutOfLives() => lives <= 0;
    public bool IsDead()       => isDead || isEliminated;
    public void AddLife()      { lives = Mathf.Min(lives + 1, 3); }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isEliminated) return;

        if (collision.gameObject.CompareTag("BulletP1") ||
            collision.gameObject.CompareTag("BulletP2") ||
            collision.gameObject.CompareTag("BulletP3"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            float  dmg    = bullet != null ? bullet.damage : 1f;
            TakeDamage(dmg);
            Destroy(collision.gameObject);
        }
    }
}