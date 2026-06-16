using UnityEngine;

public enum PowerUpType { Health, Shield, JumpBoost, SpeedBoost, Invisible, SwapPosition, AmmoRefill }

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType;
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager tidak ditemukan di scene! Pastikan ada GameObject dengan tag 'Audio' dan komponen AudioManager.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // === CEK APAKAH PEMAIN SUDAH MATI ===
            // Jika pemain sudah mati (isDead atau isEliminated), abaikan power-up sama sekali.
            Health   playerHealth   = collision.GetComponent<Health>();
            HealthP2 playerHealthP2 = collision.GetComponent<HealthP2>();
            HealthP3 playerHealthP3 = collision.GetComponent<HealthP3>();
            HealthP4 playerHealthP4 = collision.GetComponent<HealthP4>();

            bool playerIsDead = (playerHealth   != null && playerHealth.IsDead())   ||
                                (playerHealthP2 != null && playerHealthP2.IsDead()) ||
                                (playerHealthP3 != null && playerHealthP3.IsDead()) ||
                                (playerHealthP4 != null && playerHealthP4.IsDead());

            if (playerIsDead)
            {
                Debug.Log($"Power-up {powerUpType} diabaikan — pemain {collision.gameObject.name} sudah mati.");
                return; // Jangan lakukan apapun, biarkan power-up tetap ada di dunia
            }
            // ====================================
            if (audioManager != null)
            {
                AudioClip sfxToPlay = null;
                switch (powerUpType)
                {
                    case PowerUpType.Health:
                        sfxToPlay = audioManager.healthPowerUp;
                        break;
                    case PowerUpType.Shield:
                        sfxToPlay = audioManager.shieldPowerUp;
                        break;
                    case PowerUpType.JumpBoost:
                        sfxToPlay = audioManager.jumpBoostPowerUp;
                        break;
                    case PowerUpType.SpeedBoost:
                        sfxToPlay = audioManager.speedBoostPowerUp;
                        break;
                    case PowerUpType.Invisible:
                        sfxToPlay = audioManager.invisiblePowerUp;
                        break;
                    case PowerUpType.SwapPosition:
                        sfxToPlay = audioManager.swapPositionPowerUp;
                        break;
                    case PowerUpType.AmmoRefill:
                        sfxToPlay = audioManager.ammoRefillPowerUp;
                        break;
                }

                if (sfxToPlay != null)
                {
                    audioManager.PlaySFX(sfxToPlay);
                }
                else
                {
                    Debug.LogWarning($"SFX untuk power-up {powerUpType} tidak diassign di AudioManager!");
                    // Fallback ke SFX jump jika SFX spesifik tidak ada
                    if (audioManager.jump != null)
                    {
                        audioManager.PlaySFX(audioManager.jump);
                    }
                }
            }
            else
            {
                Debug.LogError("AudioManager tidak tersedia saat power-up diambil!");
            }

            // Terapkan efek power-up berdasarkan tipe
            PlayeJump2D          jumpScript     = collision.GetComponent<PlayeJump2D>();
            PlayerMovement       movementScript = collision.GetComponent<PlayerMovement>();
            PlayerPowerUpEffects effects        = collision.GetComponent<PlayerPowerUpEffects>();

            switch (powerUpType)
            {
                case PowerUpType.Health:
                    if      (playerHealth   != null) playerHealth.TakeDamage(-1f);
                    else if (playerHealthP2 != null) playerHealthP2.TakeDamage(-1f);
                    else if (playerHealthP3 != null) playerHealthP3.TakeDamage(-1f); // P3
                    else if (playerHealthP4 != null) playerHealthP4.TakeDamage(-1f); // P4
                    break;

                case PowerUpType.Shield:
                    if      (playerHealth   != null) playerHealth.GetComponent<PlayerPowerUpEffects>().ApplyShield(2);
                    else if (playerHealthP2 != null) playerHealthP2.GetComponent<PlayerPowerUpEffects>().ApplyShield(2);
                    else if (playerHealthP3 != null) playerHealthP3.GetComponent<PlayerPowerUpEffects>().ApplyShield(2); // P3
                    else if (playerHealthP4 != null) playerHealthP4.GetComponent<PlayerPowerUpEffects>().ApplyShield(2); // P4
                    break;

                case PowerUpType.JumpBoost:
                    if (jumpScript != null)
                        jumpScript.GetComponent<PlayerPowerUpEffects>().ApplyJumpBoost(1.5f, 10f);
                    break;

                case PowerUpType.SpeedBoost:
                    if (movementScript != null)
                        movementScript.GetComponent<PlayerPowerUpEffects>().ApplySpeedBoost(1.5f, 10f);
                    break;

                case PowerUpType.Invisible:
                    if (effects != null)
                        effects.ApplyInvisible(3f);
                    break;

                case PowerUpType.SwapPosition:
                    if (effects != null)
                        effects.ApplySwapPosition(3f);
                    break;

                case PowerUpType.AmmoRefill:
                    if (effects != null)
                        effects.ApplyAmmoRefill();
                    break;
            }

            // Hancurkan power-up setelah diambil
            Debug.Log($"Power-up {powerUpType} diambil oleh {collision.gameObject.name}");
            Destroy(gameObject);
        }
    }
}