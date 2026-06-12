// PlayerPowerUpEffects.cs — Updated: ApplySuddenDeathBoosts() / RemoveSuddenDeathBoosts()
// Perubahan:
//   - Tambah ApplySuddenDeathBoosts(): apply speed+jump boost permanen (tanpa timer/coroutine)
//   - Tambah RemoveSuddenDeathBoosts(): kembalikan ke nilai original
//   - Kedua method ini tidak menyentuh UI text, hanya memberi sinyal ShowIconOnly ke timerUI
// File ini menggantikan: PowerUp/PlayerPowerUpEffects.cs

using UnityEngine;
using System.Collections;

public class PlayerPowerUpEffects : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float speedBoostMultiplier = 2.0f;
    [SerializeField] private float speedBoostDuration   = 10f;

    [Header("Jump Boost Settings")]
    [SerializeField] private float jumpBoostMultiplier = 1.5f;
    [SerializeField] private float jumpBoostDuration   = 10f;

    [Header("Shield Settings")]
    [SerializeField] private int   maxShieldHits   = 2;
    [SerializeField] private float shieldDuration  = 7f;

    [Header("UI Reference")]
    [SerializeField] private PowerUpTimerUI timerUI;
    [SerializeField] private bool isPlayer1 = true;

    // Player index (1-4) untuk multi-player UI — set ini di Inspector
    // P1 = 1, P2 = 2, P3 = 3, P4 = 4
    // isPlayer1 tetap dipakai untuk backward compat (P1=true, P2=false)
    // playerIndex dipakai untuk P3/P4
    [SerializeField] private int playerIndex = 1;

    private int  shieldHitsRemaining = 0;
    private bool isShieldActive      = false;
    private Coroutine shieldCoroutine;

    private PlayeJump2D     jumpScript;
    private PlayerMovement  movementScript;
    private float originalJumpForce;
    private float originalMoveSpeed;

    private bool isSpeedBoostActive = false;
    private Coroutine speedBoostCoroutine;
    private bool isJumpBoostActive  = false;
    private Coroutine jumpBoostCoroutine;

    // Sudden Death boost — permanen sampai RemoveSuddenDeathBoosts() dipanggil
    private bool isSuddenDeathBoostActive = false;

    private void Awake()
    {
        jumpScript     = GetComponent<PlayeJump2D>();
        movementScript = GetComponent<PlayerMovement>();
        originalJumpForce = jumpScript.jumpForce;
        originalMoveSpeed = movementScript.moveSpeed;

        // timerUI harus di-assign di Inspector langsung ke PowerUpTimer milik player ini.
        // FindObjectOfType sengaja TIDAK dipakai karena ada 4 instance di scene
        // dan FindObjectOfType hanya menemukan 1 (yang pertama).
        if (timerUI == null)
            Debug.LogWarning($"[PlayerPowerUpEffects] timerUI belum di-assign di Inspector! (P{playerIndex})");
    }

    // ─── Shield ───────────────────────────────────────────────────────────────
    public void ApplyShield(int hits)
    {
        shieldHitsRemaining = Mathf.Min(hits, maxShieldHits);

        if (isShieldActive && shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        isShieldActive = true;
        shieldCoroutine = StartCoroutine(ShieldCoroutine(shieldDuration));

        if (timerUI != null)
        {
            timerUI.UpdateShieldHits(shieldHitsRemaining, isPlayer1);
            timerUI.StartShieldTimer(shieldDuration, isPlayer1);
        }
    }

    private IEnumerator ShieldCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        shieldHitsRemaining = 0;
        isShieldActive      = false;
        shieldCoroutine     = null;
        if (timerUI != null)
            timerUI.UpdateShieldHits(shieldHitsRemaining, isPlayer1);
    }

    public bool HasShield()
    {
        if (shieldHitsRemaining > 0 && isShieldActive)
        {
            shieldHitsRemaining--;
            if (timerUI != null)
                timerUI.UpdateShieldHits(shieldHitsRemaining, isPlayer1);

            if (shieldHitsRemaining <= 0)
            {
                if (shieldCoroutine != null) { StopCoroutine(shieldCoroutine); shieldCoroutine = null; }
                isShieldActive = false;
                if (timerUI != null) timerUI.ResetShieldTimer(isPlayer1);
            }
            return true;
        }
        return false;
    }

    // ─── Jump Boost ───────────────────────────────────────────────────────────
    public void ApplyJumpBoost(float multiplier, float duration)
    {
        if (isSuddenDeathBoostActive) return; // SD boost sudah aktif, skip

        if (isJumpBoostActive && jumpBoostCoroutine != null)
            StopCoroutine(jumpBoostCoroutine);
        else
            jumpScript.jumpForce *= jumpBoostMultiplier;

        isJumpBoostActive  = true;
        jumpBoostCoroutine = StartCoroutine(JumpBoostCoroutine(jumpBoostMultiplier, jumpBoostDuration));

        if (timerUI != null)
            timerUI.StartJumpBoostTimer(jumpBoostDuration, isPlayer1);
    }

    private IEnumerator JumpBoostCoroutine(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        jumpScript.jumpForce = originalJumpForce;
        isJumpBoostActive    = false;
        jumpBoostCoroutine   = null;
    }

    // ─── Speed Boost ──────────────────────────────────────────────────────────
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (isSuddenDeathBoostActive) return; // SD boost sudah aktif, skip

        if (isSpeedBoostActive && speedBoostCoroutine != null)
            StopCoroutine(speedBoostCoroutine);
        else
            movementScript.moveSpeed *= speedBoostMultiplier;

        isSpeedBoostActive  = true;
        speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine(speedBoostMultiplier, speedBoostDuration));

        if (timerUI != null)
            timerUI.StartSpeedBoostTimer(speedBoostDuration, isPlayer1);
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        movementScript.moveSpeed = originalMoveSpeed;
        isSpeedBoostActive       = false;
        speedBoostCoroutine      = null;
    }

    // ─── Sudden Death Boosts (permanen, tanpa timer text) ─────────────────────
    // Dipanggil GameManager saat Sudden Death dimulai untuk semua peserta.
    // Terapkan speed + jump boost permanen, tampilkan hanya icon UI (tanpa text timer).
    public void ApplySuddenDeathBoosts()
    {
        if (isSuddenDeathBoostActive) return;

        // Hentikan boost normal yang mungkin masih berjalan
        if (isSpeedBoostActive && speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
            speedBoostCoroutine = null;
            isSpeedBoostActive  = false;
            movementScript.moveSpeed = originalMoveSpeed; // reset dulu
        }
        if (isJumpBoostActive && jumpBoostCoroutine != null)
        {
            StopCoroutine(jumpBoostCoroutine);
            jumpBoostCoroutine = null;
            isJumpBoostActive  = false;
            jumpScript.jumpForce = originalJumpForce; // reset dulu
        }

        // Terapkan boost permanen
        movementScript.moveSpeed = originalMoveSpeed * speedBoostMultiplier;
        jumpScript.jumpForce     = originalJumpForce * jumpBoostMultiplier;
        isSuddenDeathBoostActive = true;

        // Tampilkan icon UI tanpa timer text
        if (timerUI != null)
            timerUI.ShowSuddenDeathBoostIcons(playerIndex);

        Debug.Log($"[SD Boost] P{playerIndex}: speed={movementScript.moveSpeed}, jump={jumpScript.jumpForce}");
    }

    // Dipanggil saat game berakhir (dari EndSuddenDeathRound / ResetPowerUps).
    public void RemoveSuddenDeathBoosts()
    {
        if (!isSuddenDeathBoostActive) return;

        movementScript.moveSpeed = originalMoveSpeed;
        jumpScript.jumpForce     = originalJumpForce;
        isSuddenDeathBoostActive = false;

        if (timerUI != null)
            timerUI.HideSuddenDeathBoostIcons(playerIndex);

        Debug.Log($"[SD Boost] P{playerIndex}: boost dihapus.");
    }

    // ─── Reset All ────────────────────────────────────────────────────────────
    public void ResetPowerUps()
    {
        // Shield
        shieldHitsRemaining = 0;
        isShieldActive      = false;
        if (shieldCoroutine != null) { StopCoroutine(shieldCoroutine); shieldCoroutine = null; }
        if (timerUI != null) { timerUI.UpdateShieldHits(0, isPlayer1); timerUI.ResetShieldTimer(isPlayer1); }

        // Jump boost
        if (isJumpBoostActive && jumpBoostCoroutine != null) { StopCoroutine(jumpBoostCoroutine); jumpBoostCoroutine = null; }
        isJumpBoostActive    = false;
        jumpScript.jumpForce = originalJumpForce;
        if (timerUI != null) timerUI.ResetJumpBoostTimer(isPlayer1);

        // Speed boost
        if (isSpeedBoostActive && speedBoostCoroutine != null) { StopCoroutine(speedBoostCoroutine); speedBoostCoroutine = null; }
        isSpeedBoostActive       = false;
        movementScript.moveSpeed = originalMoveSpeed;
        if (timerUI != null) timerUI.ResetSpeedBoostTimer(isPlayer1);

        // SD boost
        RemoveSuddenDeathBoosts();
    }
}