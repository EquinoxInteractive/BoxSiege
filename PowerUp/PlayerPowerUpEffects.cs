// PlayerPowerUpEffects.cs — Updated: tambah ApplyInvisible(), ApplySwapPosition(), ApplyAmmoRefill()
// Perubahan:
//   - Tambah ApplyInvisible(duration): sembunyikan SpriteRenderer player + senjata + melee
//     selama durasi tertentu. Peluru yang ditembak selama mode ini juga invisible
//     (ditangani oleh PlayerShooting & Bullet).
//   - Tambah ApplySwapPosition(delay): mulai window konfirmasi (dipanggil dengan 3 detik dari PowerUp.cs).
//     Jika tombol Shoot (OnShootPressed) ditekan dalam window tersebut, lakukan
//     swap posisi (lihat PerformSwapPosition()). Jika tidak, power-up hilang.
//   - Tambah ApplyAmmoRefill(): instant, panggil PlayerShooting.RefillAmmo().
//   - Tambah ResetPowerUps() menangani reset utk power-up baru.
// File ini menggantikan: PowerUp/PlayerPowerUpEffects.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private PlayerShooting  shootingScript;
    private SpriteRenderer  spriteRenderer;
    private AudioManager    audioManager;
    private float originalJumpForce;
    private float originalMoveSpeed;

    private bool isSpeedBoostActive = false;
    private Coroutine speedBoostCoroutine;
    private bool isJumpBoostActive  = false;
    private Coroutine jumpBoostCoroutine;

    // Sudden Death boost — permanen sampai RemoveSuddenDeathBoosts() dipanggil
    private bool isSuddenDeathBoostActive = false;

    // ─── Invisible ────────────────────────────────────────────────────────────
    private bool isInvisibleActive = false;
    private Coroutine invisibleCoroutine;

    // ─── Swap Position ────────────────────────────────────────────────────────
    private bool isSwapWindowActive = false;
    private Coroutine swapWindowCoroutine;

    private void Awake()
    {
        jumpScript     = GetComponent<PlayeJump2D>();
        movementScript = GetComponent<PlayerMovement>();
        shootingScript = GetComponent<PlayerShooting>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioManager    = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>();
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

    // Coba serap sebanyak mungkin dari `incomingDamage` menggunakan shield.
    // Setiap charge shield menyerap 1 unit damage.
    //
    // RETURN VALUE:
    //   Jumlah damage yang berhasil diserap shield (0 jika tidak ada shield).
    //   Sisa damage = incomingDamage - returnValue, diteruskan ke health.
    //
    // Contoh: shield sisa 1, incomingDamage = 3
    //   -> return 1, sisa damage ke health = 2, shield habis.
    // Contoh: shield sisa 2, incomingDamage = 2
    //   -> return 2, sisa damage ke health = 0, shield habis.
    // Contoh: tidak ada shield, incomingDamage = apapun
    //   -> return 0, seluruh damage ke health.
    public int AbsorbWithShield(float incomingDamage)
    {
        if (!isShieldActive || shieldHitsRemaining <= 0) return 0;

        int canAbsorb = Mathf.Min(shieldHitsRemaining, Mathf.FloorToInt(incomingDamage));
        if (canAbsorb <= 0) return 0;

        shieldHitsRemaining -= canAbsorb;

        if (timerUI != null)
            timerUI.UpdateShieldHits(shieldHitsRemaining, isPlayer1);

        if (shieldHitsRemaining <= 0)
        {
            if (shieldCoroutine != null) { StopCoroutine(shieldCoroutine); shieldCoroutine = null; }
            isShieldActive = false;
            if (timerUI != null) timerUI.ResetShieldTimer(isPlayer1);
        }

        return canAbsorb;
    }

    // Backward-compat untuk pemanggil lama (bullet, melee) yang hanya butuh
    // tahu "ada shield atau tidak" dan mengonsumsi tepat 1 charge.
    public bool HasShield()
    {
        return AbsorbWithShield(1f) > 0;
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

    // ─── Invisible ────────────────────────────────────────────────────────────
    // Sembunyikan player (sprite badan, senjata, melee) selama `duration` detik.
    // Pemain masih bisa bergerak & menembak, sound effect tetap berjalan.
    // Peluru yang ditembak selama mode ini juga otomatis invisible
    // (lihat PlayerShooting.Shoot() & Bullet.SetInvisible()).
    public void ApplyInvisible(float duration)
    {
        if (isInvisibleActive && invisibleCoroutine != null)
        {
            StopCoroutine(invisibleCoroutine);
        }
        else
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }

        if (shootingScript != null) shootingScript.SetInvisible(true);

        isInvisibleActive  = true;
        invisibleCoroutine = StartCoroutine(InvisibleCoroutine(duration));

        if (timerUI != null)
            timerUI.StartInvisibleTimer(duration, isPlayer1);
    }

    private IEnumerator InvisibleCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndInvisible();
    }

    private void EndInvisible()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (shootingScript != null) shootingScript.SetInvisible(false);

        isInvisibleActive  = false;
        invisibleCoroutine = null;

        if (timerUI != null)
            timerUI.ResetInvisibleTimer(isPlayer1);
    }

    // ─── Swap Position ────────────────────────────────────────────────────────
    // Saat power-up diambil, buka "window" konfirmasi selama `delay` detik.
    // Jika pemain menekan tombol Shoot dalam window tersebut, lakukan swap
    // posisi (lihat PerformSwapPosition di GroupSwap class di bawah).
    // Jika tidak ditekan, power-up hilang tanpa efek.
    public void ApplySwapPosition(float delay)
    {
        if (isSwapWindowActive && swapWindowCoroutine != null)
        {
            StopCoroutine(swapWindowCoroutine);
        }

        isSwapWindowActive  = true;
        swapWindowCoroutine = StartCoroutine(SwapWindowCoroutine(delay));

        if (timerUI != null)
            timerUI.StartSwapPositionTimer(delay, isPlayer1);
    }

    private IEnumerator SwapWindowCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Window habis tanpa konfirmasi — power-up hilang
        if (isSwapWindowActive)
        {
            isSwapWindowActive  = false;
            swapWindowCoroutine = null;
            if (timerUI != null)
                timerUI.ResetSwapPositionTimer(isPlayer1);

            Debug.Log($"[SwapPosition] P{playerIndex}: window habis, power-up hilang.");
        }
    }

    // Dipanggil oleh PlayerShooting.OnShoot SEBELUM logika tembak/melee
    // dijalankan untuk frame yang sama. Jika window swap aktif:
    //  - Konsumsi window (nonaktifkan, hentikan coroutine timeout, reset UI)
    //  - Lakukan swap posisi
    //  - Kembalikan true agar PlayerShooting MEMBATALKAN aksi tembak/melee
    //    pada penekanan tombol ini (mencegah "swap-kill").
    // Jika window tidak aktif, kembalikan false dan PlayerShooting
    // melanjutkan logika tembak/melee seperti biasa.
    public bool TryConsumeSwapWindow()
    {
        if (!isSwapWindowActive) return false;

        // Konsumsi window — power-up dipakai
        isSwapWindowActive = false;
        if (swapWindowCoroutine != null) { StopCoroutine(swapWindowCoroutine); swapWindowCoroutine = null; }
        if (timerUI != null) timerUI.ResetSwapPositionTimer(isPlayer1);

        // SFX saat swap benar-benar terjadi (berbeda dari SFX saat power-up diambil)
        if (audioManager != null && audioManager.swapPositionActivate != null)
            audioManager.PlaySFX(audioManager.swapPositionActivate);

        SwapPositionExecutor.PerformSwap();
        return true;
    }

    // ─── Ammo Refill ──────────────────────────────────────────────────────────
    // Instant (seperti Heal): isi ulang peluru senjata ke nilai maxAmmo.
    // Jika sedang menggunakan secondary weapon, kembalikan ke primary
    // dengan peluru penuh (lihat PlayerShooting.RefillAmmo()).
    public void ApplyAmmoRefill()
    {
        if (shootingScript != null)
            shootingScript.RefillAmmo();
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

        // Invisible
        if (invisibleCoroutine != null) { StopCoroutine(invisibleCoroutine); invisibleCoroutine = null; }
        if (isInvisibleActive) EndInvisible();

        // Swap Position window
        if (swapWindowCoroutine != null) { StopCoroutine(swapWindowCoroutine); swapWindowCoroutine = null; }
        isSwapWindowActive = false;
        if (timerUI != null) timerUI.ResetSwapPositionTimer(isPlayer1);

        // SD boost
        RemoveSuddenDeathBoosts();
    }
}

// ─── Swap Position Executor ────────────────────────────────────────────────
// Helper statis untuk menukar posisi (Vector3) seluruh pemain yang masih hidup
// di scene. Tidak perlu di-attach ke GameObject manapun — dipanggil langsung
// secara statis dari PlayerPowerUpEffects.
//
// Aturan:
//  - 2 pemain hidup  : P1 <-> P2 (tukar posisi langsung)
//  - 3/4 pemain hidup: semua pemain pasti bertukar posisi secara acak
//                      (derangement / permutasi acak tanpa titik tetap).
//  - Hanya posisi (x, y, z) yang ditukar. Arah hadap (facing) tidak diubah.
//  - Pemain yang sudah mati/eliminated tidak ikut dalam swap.
public static class SwapPositionExecutor
{
    public static void PerformSwap()
    {
        List<Transform> alivePlayers = GetAlivePlayerTransforms();

        if (alivePlayers.Count < 2)
        {
            Debug.Log("[SwapPosition] Tidak cukup pemain hidup untuk swap (minimal 2).");
            return;
        }

        // Simpan posisi asli semua pemain
        Vector3[] originalPositions = new Vector3[alivePlayers.Count];
        for (int i = 0; i < alivePlayers.Count; i++)
            originalPositions[i] = alivePlayers[i].position;

        if (alivePlayers.Count == 2)
        {
            // 2 pemain: tukar langsung
            alivePlayers[0].position = originalPositions[1];
            alivePlayers[1].position = originalPositions[0];
        }
        else
        {
            // 3 atau 4 pemain: permutasi acak tanpa titik tetap (derangement)
            int[] mapping = GenerateDerangement(alivePlayers.Count);

            for (int i = 0; i < alivePlayers.Count; i++)
            {
                alivePlayers[i].position = originalPositions[mapping[i]];
            }
        }

        Debug.Log($"[SwapPosition] Swap posisi dilakukan untuk {alivePlayers.Count} pemain.");
    }

    // Mengembalikan list Transform pemain yang masih hidup (P1-P4),
    // urutan tetap konsisten P1, P2, P3, P4.
    private static List<Transform> GetAlivePlayerTransforms()
    {
        List<Transform> result = new List<Transform>();

        Health p1 = Object.FindObjectOfType<Health>();
        if (p1 != null && !p1.IsDead()) result.Add(p1.transform);

        HealthP2 p2 = Object.FindObjectOfType<HealthP2>();
        if (p2 != null && !p2.IsDead()) result.Add(p2.transform);

        HealthP3 p3 = Object.FindObjectOfType<HealthP3>();
        if (p3 != null && !p3.IsDead()) result.Add(p3.transform);

        HealthP4 p4 = Object.FindObjectOfType<HealthP4>();
        if (p4 != null && !p4.IsDead()) result.Add(p4.transform);

        return result;
    }

    // Menghasilkan permutasi acak dari [0..n-1] di mana tidak ada elemen
    // yang memetakan ke dirinya sendiri (derangement). Untuk n >= 2 selalu
    // ada solusi. Menggunakan algoritma shuffle + perbaikan fixed point.
    private static int[] GenerateDerangement(int n)
    {
        int[] arr = new int[n];
        for (int i = 0; i < n; i++) arr[i] = i;

        // Coba shuffle berulang sampai tidak ada fixed point (i == arr[i]).
        // Untuk n kecil (3-4) ini sangat cepat.
        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Shuffle(arr);

            bool hasFixedPoint = false;
            for (int i = 0; i < n; i++)
            {
                if (arr[i] == i) { hasFixedPoint = true; break; }
            }

            if (!hasFixedPoint) return arr;
        }

        // Fallback: jika gagal (sangat tidak mungkin untuk n>=2),
        // gunakan rotasi sederhana (cyclic shift) yang dijamin tanpa fixed point.
        int[] fallback = new int[n];
        for (int i = 0; i < n; i++) fallback[i] = (i + 1) % n;
        return fallback;
    }

    private static void Shuffle(int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
    }
}