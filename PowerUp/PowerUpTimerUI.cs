// PowerUpTimerUI.cs — Updated: tambah slot UI untuk Invisible & Swap Position
// 1 instance per player. Assign di Inspector:
//   Speed Boost Icon     → parent GO icon speed (di-show saat speed aktif)
//   Speed Boost Text     → TMP child dari icon (di-hide saat SD)
//   Jump Boost Icon      → parent GO icon jump
//   Jump Boost Text      → TMP child dari icon
//   Shield Icon          → parent GO icon shield
//   Shield Hits Text     → TMP child dari icon
//   Invisible Icon       → parent GO icon invisible (BARU)
//   Invisible Text       → TMP child dari icon invisible (BARU)
//   Swap Position Icon   → parent GO icon swap position (BARU)
//   Swap Position Text   → TMP child dari icon swap position (BARU)
//
// TIDAK perlu slot tambahan untuk Sudden Death — pakai icon yang sama.
// Ammo Refill TIDAK memiliki timer UI karena bersifat instan (seperti Heal).
// File ini menggantikan: PowerUp/PowerUpTimerUI.cs

using UnityEngine;
using TMPro;

public class PowerUpTimerUI : MonoBehaviour
{
    [Header("Speed Boost UI")]
    [SerializeField] private GameObject      speedBoostIcon;
    [SerializeField] private TextMeshProUGUI speedBoostTimerText;

    [Header("Jump Boost UI")]
    [SerializeField] private GameObject      jumpBoostIcon;
    [SerializeField] private TextMeshProUGUI jumpBoostTimerText;

    [Header("Shield UI")]
    [SerializeField] private GameObject      shieldIcon;
    [SerializeField] private TextMeshProUGUI shieldHitsText;

    [Header("Invisible UI")]
    [SerializeField] private GameObject      invisibleIcon;
    [SerializeField] private TextMeshProUGUI invisibleTimerText;

    [Header("Swap Position UI")]
    [SerializeField] private GameObject      swapPositionIcon;
    [SerializeField] private TextMeshProUGUI swapPositionTimerText;

    // ─── State ────────────────────────────────────────────────────────────────
    private float speedRemainingTime  = 0f;
    private float jumpRemainingTime   = 0f;
    private float shieldRemainingTime = 0f;
    private int   shieldHitsCount     = 0;
    private float invisibleRemainingTime     = 0f;
    private float swapPositionRemainingTime  = 0f;

    private bool speedBoostActive    = false;
    private bool jumpBoostActive     = false;
    private bool shieldBoostActive   = false;
    private bool invisibleActive     = false;
    private bool swapPositionActive  = false;

    // Saat true: icon speed+jump menyala permanen, text disembunyikan, countdown berhenti
    private bool isSuddenDeathMode = false;

    private void Awake()
    {
        SetGO(speedBoostIcon, false);
        SetGO(jumpBoostIcon,  false);
        SetGO(shieldIcon,     false);
        SetGO(invisibleIcon,  false);
        SetGO(swapPositionIcon, false);
        SetGO(speedBoostTimerText?.gameObject, false);
        SetGO(jumpBoostTimerText?.gameObject,  false);
        SetGO(shieldHitsText?.gameObject,      false);
        SetGO(invisibleTimerText?.gameObject,      false);
        SetGO(swapPositionTimerText?.gameObject,   false);
    }

    private void Update()
    {
        // Speed countdown — hanya jalan di luar Sudden Death
        if (speedBoostActive && !isSuddenDeathMode)
        {
            speedRemainingTime -= Time.deltaTime;
            if (speedRemainingTime > 0)
            {
                SetText(speedBoostTimerText, $"{Mathf.CeilToInt(speedRemainingTime)}s");
            }
            else
            {
                // Waktu habis: sembunyikan icon DAN text sekaligus
                speedBoostActive = false;
                SetGO(speedBoostIcon,                  false);
                SetGO(speedBoostTimerText?.gameObject, false);
            }
        }

        // Jump countdown
        if (jumpBoostActive && !isSuddenDeathMode)
        {
            jumpRemainingTime -= Time.deltaTime;
            if (jumpRemainingTime > 0)
            {
                SetText(jumpBoostTimerText, $"{Mathf.CeilToInt(jumpRemainingTime)}s");
            }
            else
            {
                jumpBoostActive = false;
                SetGO(jumpBoostIcon,                  false);
                SetGO(jumpBoostTimerText?.gameObject, false);
            }
        }

        // Shield countdown
        if (shieldBoostActive)
        {
            shieldRemainingTime -= Time.deltaTime;
            if (shieldRemainingTime > 0)
            {
                SetText(shieldHitsText, $"{shieldHitsCount}x | {Mathf.CeilToInt(shieldRemainingTime)}s");
            }
            else
            {
                shieldBoostActive = false;
                shieldHitsCount   = 0;
                SetGO(shieldIcon,                false);
                SetGO(shieldHitsText?.gameObject, false);
            }
        }

        // Invisible countdown
        if (invisibleActive)
        {
            invisibleRemainingTime -= Time.deltaTime;
            if (invisibleRemainingTime > 0)
            {
                SetText(invisibleTimerText, $"{Mathf.CeilToInt(invisibleRemainingTime)}s");
            }
            else
            {
                invisibleActive = false;
                SetGO(invisibleIcon,                  false);
                SetGO(invisibleTimerText?.gameObject, false);
            }
        }

        // Swap Position countdown
        if (swapPositionActive)
        {
            swapPositionRemainingTime -= Time.deltaTime;
            if (swapPositionRemainingTime > 0)
            {
                SetText(swapPositionTimerText, $"{Mathf.CeilToInt(swapPositionRemainingTime)}s");
            }
            else
            {
                swapPositionActive = false;
                SetGO(swapPositionIcon,                  false);
                SetGO(swapPositionTimerText?.gameObject, false);
            }
        }
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void StartSpeedBoostTimer(float duration, bool isPlayer1 = true)
    {
        if (isSuddenDeathMode) return; // SD boost sudah aktif, jangan override
        speedRemainingTime = duration;
        speedBoostActive   = true;
        SetText(speedBoostTimerText, $"{Mathf.CeilToInt(duration)}s");
        SetGO(speedBoostIcon,                  true);
        SetGO(speedBoostTimerText?.gameObject, true);
    }

    public void StartJumpBoostTimer(float duration, bool isPlayer1 = true)
    {
        if (isSuddenDeathMode) return;
        jumpRemainingTime = duration;
        jumpBoostActive   = true;
        SetText(jumpBoostTimerText, $"{Mathf.CeilToInt(duration)}s");
        SetGO(jumpBoostIcon,                  true);
        SetGO(jumpBoostTimerText?.gameObject, true);
    }

    public void StartShieldTimer(float duration, bool isPlayer1 = true)
    {
        shieldRemainingTime = duration;
        shieldBoostActive   = true;
        SetGO(shieldIcon,                true);
        SetGO(shieldHitsText?.gameObject, true);
    }

    public void UpdateShieldHits(int hits, bool isPlayer1 = true)
    {
        shieldHitsCount = hits;
        if (hits <= 0 && !shieldBoostActive)
        {
            SetGO(shieldIcon,                false);
            SetGO(shieldHitsText?.gameObject, false);
        }
        else if (shieldBoostActive)
        {
            SetText(shieldHitsText,
                $"{hits}x | {Mathf.CeilToInt(Mathf.Max(shieldRemainingTime, 0))}s");
        }
    }

    // ─── Invisible ────────────────────────────────────────────────────────────
    public void StartInvisibleTimer(float duration, bool isPlayer1 = true)
    {
        invisibleRemainingTime = duration;
        invisibleActive        = true;
        SetText(invisibleTimerText, $"{Mathf.CeilToInt(duration)}s");
        SetGO(invisibleIcon,                  true);
        SetGO(invisibleTimerText?.gameObject, true);
    }

    public void ResetInvisibleTimer(bool isPlayer1 = true)
    {
        invisibleRemainingTime = 0f;
        invisibleActive        = false;
        SetGO(invisibleIcon,                  false);
        SetGO(invisibleTimerText?.gameObject, false);
    }

    // ─── Swap Position ────────────────────────────────────────────────────────
    public void StartSwapPositionTimer(float duration, bool isPlayer1 = true)
    {
        swapPositionRemainingTime = duration;
        swapPositionActive        = true;
        SetText(swapPositionTimerText, $"{Mathf.CeilToInt(duration)}s");
        SetGO(swapPositionIcon,                  true);
        SetGO(swapPositionTimerText?.gameObject, true);
    }

    public void ResetSwapPositionTimer(bool isPlayer1 = true)
    {
        swapPositionRemainingTime = 0f;
        swapPositionActive        = false;
        SetGO(swapPositionIcon,                  false);
        SetGO(swapPositionTimerText?.gameObject, false);
    }

    public void ResetSpeedBoostTimer(bool isPlayer1 = true)
    {
        speedRemainingTime = 0f;
        speedBoostActive   = false;
        if (!isSuddenDeathMode)
        {
            SetGO(speedBoostIcon,                  false);
            SetGO(speedBoostTimerText?.gameObject, false);
        }
    }

    public void ResetJumpBoostTimer(bool isPlayer1 = true)
    {
        jumpRemainingTime = 0f;
        jumpBoostActive   = false;
        if (!isSuddenDeathMode)
        {
            SetGO(jumpBoostIcon,                  false);
            SetGO(jumpBoostTimerText?.gameObject, false);
        }
    }

    public void ResetShieldTimer(bool isPlayer1 = true)
    {
        shieldRemainingTime = 0f;
        shieldBoostActive   = false;
        shieldHitsCount     = 0;
        SetGO(shieldIcon,                false);
        SetGO(shieldHitsText?.gameObject, false);
    }

    // ─── Sudden Death ─────────────────────────────────────────────────────────
    // Tampilkan icon Speed+Jump permanen tanpa text timer.
    // Menggunakan icon yang sama dengan boost normal.
    public void ShowSuddenDeathBoostIcons(int playerIndex = 1)
    {
        isSuddenDeathMode = true;

        // Hentikan countdown normal
        speedBoostActive = false;
        jumpBoostActive  = false;
        speedRemainingTime = 0f;
        jumpRemainingTime  = 0f;

        // Tampilkan icon, sembunyikan text
        SetGO(speedBoostIcon,                  true);
        SetGO(speedBoostTimerText?.gameObject, false);

        SetGO(jumpBoostIcon,                  true);
        SetGO(jumpBoostTimerText?.gameObject, false);

        Debug.Log($"[PowerUpTimerUI] ShowSuddenDeathBoostIcons P{playerIndex} — speedIcon={speedBoostIcon?.name}, jumpIcon={jumpBoostIcon?.name}");
    }

    public void HideSuddenDeathBoostIcons(int playerIndex = 1)
    {
        isSuddenDeathMode = false;
        SetGO(speedBoostIcon,                  false);
        SetGO(speedBoostTimerText?.gameObject, false);
        SetGO(jumpBoostIcon,                  false);
        SetGO(jumpBoostTimerText?.gameObject, false);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private void SetGO(GameObject go, bool active)
    { if (go != null) go.SetActive(active); }

    private void SetText(TextMeshProUGUI t, string txt)
    { if (t != null) t.text = txt; }
}