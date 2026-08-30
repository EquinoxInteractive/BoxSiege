using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// UIPanelAnimator.cs
// Script UMUM (reusable) untuk animasi buka/tutup Panel dengan LeanTween.
// Cocok dipakai di HAMPIR SEMUA Panel di game ini (Pause, Win/Game Over, Settings,
// Character Selection, Map Selection, dsb) yang punya struktur:
//   Panel (root, GameObject ini sendiri)
//   └─ Background (Image full screen + CanvasGroup, untuk efek gelap/fade)
//   └─ Box (RectTransform isi kontennya, kotak dialog / menu)
//
// CARA ASSIGN DI UNITY:
// 1. Pilih GameObject "Panel" kamu di Hierarchy.
// 2. Add Component -> UIPanelAnimator.
// 3. Drag CanvasGroup dari GameObject background ke field "Background".
//    (Kalau belum ada CanvasGroup, Add Component -> Canvas Group di GameObject background-nya.)
// 4. Drag RectTransform dari GameObject box/kontennya ke field "Box".
// 5. Pilih "Style" animasi yang diinginkan di Inspector.
// 6. "Use Unscaled Time" boleh dicentang manual untuk memaksa unscaled time, TAPI
//    SEKARANG TIDAK WAJIB LAGI — lihat catatan "FIX BUG PAUSE" di bawah.
// 7. Panggil lewat script lain: panelAnimator.Open() dan panelAnimator.Close()
//    (gantikan pauseMenuPanel.SetActive(true/false) dengan ini).
//
// ============================== FIX BUG: ANIMASI MACET SAAT SCENE MAP DI-PAUSE ==============================
// Sebelumnya, animasi HANYA berjalan lancar saat panel muncul di Main Menu / Character
// Selection karena scene-scene itu tidak pernah men-set Time.timeScale = 0.
//
// Begitu komponen ini dipasang di Panel yang muncul di scene Map (misal "Pause Panel" di
// TheDessert) — di mana PauseMenuManager men-set Time.timeScale = 0f DULU sebelum panel
// dibuka — animasi jadi macet/diam total. Penyebabnya: LeanTween secara default memakai
// SCALED delta time (Time.deltaTime), dan Time.deltaTime = 0 setiap frame ketika
// Time.timeScale = 0. Field "useUnscaledTime" di Inspector memang bisa memperbaiki ini,
// TAPI harus dicentang MANUAL satu per satu di SETIAP Panel di SETIAP scene Map — sangat
// gampang lupa (persis yang terjadi di scene TheDessert, field ini masih OFF).
//
// SEKARANG: ditambahkan deteksi OTOMATIS lewat properti "EffectiveUnscaledTime" di bawah.
// Kalau saat Open()/Close() dipanggil ternyata Time.timeScale sedang 0 (artinya game
// sedang di-pause oleh script MANAPUN, tidak harus PauseMenuManager), animasi otomatis
// dipaksa pakai unscaled time juga — TANPA perlu centang apa pun di Inspector. Kalau
// Time.timeScale normal (1), perilaku sama persis seperti sebelumnya (mengikuti nilai
// "useUnscaledTime" yang kamu atur). Jadi ini aman dipasang ke SEMUA panel di SEMUA
// scene (Main Menu, Character Selection, maupun tiap scene Map) tanpa perlu setting
// tambahan apa pun, dan tidak akan bisa "kelupaan" lagi ke depannya.
// ================================================================================================================
[RequireComponent(typeof(RectTransform))]
public class UIPanelAnimator : MonoBehaviour
{
    public enum AnimationStyle
    {
        SlideFromBottom,
        SlideFromTop,
        SlideFromLeft,
        SlideFromRight,
        ScaleBounce,
        FadeOnly
    }

    [Header("References")]
    [Tooltip("CanvasGroup pada background gelap (opsional, boleh dikosongkan)")]
    [SerializeField] private CanvasGroup background;
    [Tooltip("RectTransform pada kotak/box konten utama panel")]
    [SerializeField] private RectTransform box;

    [Header("Style")]
    [SerializeField] private AnimationStyle style = AnimationStyle.SlideFromBottom;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float openDelay = 0f;
    [Tooltip("Jarak geser untuk style Slide, dihitung otomatis dari ukuran layar jika 0")]
    [SerializeField] private float slideDistance = 0f;
    [Tooltip("Paksa unscaled time selalu aktif. TIDAK WAJIB dicentang lagi — kalau Time.timeScale " +
             "terdeteksi 0 (game sedang pause) saat Open()/Close() dipanggil, unscaled time otomatis " +
             "dipakai walau field ini dibiarkan OFF. Centang manual di sini hanya perlu kalau kamu mau " +
             "panel ini SELALU pakai unscaled time walau game tidak sedang pause.")]
    [SerializeField] private bool useUnscaledTime = false;
    [Tooltip("Mainkan animasi Open otomatis saat GameObject di-enable")]
    [SerializeField] private bool playOnEnable = true;

    [Header("Scale Bounce (khusus style ScaleBounce)")]
    [Tooltip("Seberapa kecil box di awal sebelum membesar. 0 = titik (paling dramatis), 1 = tidak mengecil sama sekali. Coba 0.1 - 0.3 untuk efek 'pop' yang kuat.")]
    [Range(0f, 1f)]
    [SerializeField] private float scaleBounceStartScale = 0.15f;
    [Tooltip("Seberapa besar overshoot melewati ukuran normal sebelum settle (bikin efek 'boing'). 1 = tanpa overshoot, 1.15 = overshoot ringan, 1.3+ = overshoot kuat.")]
    [SerializeField] private float scaleBounceOvershoot = 1.15f;

    [Header("Events")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    private Vector2 boxOriginalAnchoredPos;
    private Vector3 boxOriginalScale = Vector3.one;
    private bool cached = false;
    private bool isAnimating = false;
    private Coroutine fallbackRoutine;

    // FIX BUG PAUSE: dipakai di semua pemanggilan LeanTween & delay fallback di script ini
    // menggantikan pemakaian langsung field "useUnscaledTime". Lihat penjelasan lengkap di
    // komentar besar bagian atas file.
    private bool EffectiveUnscaledTime => useUnscaledTime || Time.timeScale <= 0f;

    private void Awake()
    {
        CacheOriginal();
    }

    private void OnEnable()
    {
        CacheOriginal();
        if (playOnEnable) PlayOpen();
    }

    private void CacheOriginal()
    {
        if (cached) return;
        if (box != null)
        {
            boxOriginalAnchoredPos = box.anchoredPosition;
            boxOriginalScale = box.localScale;
        }
        cached = true;
    }

    private float GetSlideDistance(bool horizontal)
    {
        if (slideDistance > 0f) return slideDistance;

        // Otomatis ambil dari ukuran Canvas terdekat supaya aman di berbagai resolusi
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
                return horizontal ? canvasRect.rect.width : canvasRect.rect.height;
        }
        return horizontal ? Screen.width : Screen.height;
    }

    private Vector2 GetHiddenOffset()
    {
        switch (style)
        {
            case AnimationStyle.SlideFromBottom: return new Vector2(0f, -GetSlideDistance(false));
            case AnimationStyle.SlideFromTop: return new Vector2(0f, GetSlideDistance(false));
            case AnimationStyle.SlideFromLeft: return new Vector2(-GetSlideDistance(true), 0f);
            case AnimationStyle.SlideFromRight: return new Vector2(GetSlideDistance(true), 0f);
            default: return Vector2.zero;
        }
    }

    /// <summary>Buka panel dengan animasi (memastikan GameObject aktif dulu).</summary>
    public void Open()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        CacheOriginal();
        PlayOpen();
    }

    private void PlayOpen()
    {
        CacheOriginal();
        isAnimating = true;
        bool unscaled = EffectiveUnscaledTime;
        CancelFallback();

        if (background != null)
        {
            LeanTween.cancel(background.gameObject);
            background.alpha = 0f;
            background.LeanAlpha(1f, duration)
                .setEase(LeanTweenType.easeOutSine)
                .setIgnoreTimeScale(unscaled);
        }

        if (box != null)
        {
            LeanTween.cancel(box.gameObject);

            if (style == AnimationStyle.ScaleBounce)
            {
                box.anchoredPosition = boxOriginalAnchoredPos;
                box.localScale = boxOriginalScale * scaleBounceStartScale;

                // Tahap 1: pop cepat dari kecil sampai SEDIKIT melewati ukuran normal (overshoot)
                LeanTween.scale(box, boxOriginalScale * scaleBounceOvershoot, duration * 0.6f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setIgnoreTimeScale(unscaled)
                    .setDelay(openDelay)
                    .setOnComplete(() =>
                    {
                        // Tahap 2: settle balik pelan ke ukuran normal (efek "boing")
                        LeanTween.scale(box, boxOriginalScale, duration * 0.4f)
                            .setEase(LeanTweenType.easeOutBack)
                            .setIgnoreTimeScale(unscaled)
                            .setOnComplete(OnOpenComplete);
                    });
            }
            else if (style == AnimationStyle.FadeOnly)
            {
                box.anchoredPosition = boxOriginalAnchoredPos;
                box.localScale = boxOriginalScale;
                OnOpenComplete();
            }
            else
            {
                Vector2 hidden = boxOriginalAnchoredPos + GetHiddenOffset();
                box.anchoredPosition = hidden;
                box.localScale = boxOriginalScale;
                LeanTween.move(box, boxOriginalAnchoredPos, duration)
                    .setEase(LeanTweenType.easeOutBack)
                    .setIgnoreTimeScale(unscaled)
                    .setDelay(openDelay)
                    .setOnComplete(OnOpenComplete);
            }
        }
        else
        {
            // FIX BUG PAUSE: dulu pakai Invoke(), yang ikut mengikuti Time.timeScale (jadi
            // ikut macet saat pause). Sekarang pakai coroutine + WaitForSecondsRealtime saat
            // unscaled aktif, supaya callback tetap terpanggil walau game sedang di-pause.
            fallbackRoutine = StartCoroutine(DelayedCallback(OnOpenComplete, duration + openDelay, unscaled));
        }
    }

    private void OnOpenComplete()
    {
        isAnimating = false;
        onOpened?.Invoke();
    }

    /// <summary>Tutup panel dengan animasi, lalu otomatis SetActive(false).</summary>
    public void Close()
    {
        CacheOriginal();
        isAnimating = true;
        bool unscaled = EffectiveUnscaledTime;
        CancelFallback();

        if (background != null)
        {
            LeanTween.cancel(background.gameObject);
            background.LeanAlpha(0f, duration)
                .setEase(LeanTweenType.easeInSine)
                .setIgnoreTimeScale(unscaled);
        }

        if (box != null)
        {
            LeanTween.cancel(box.gameObject);

            if (style == AnimationStyle.ScaleBounce)
            {
                LeanTween.scale(box, boxOriginalScale * scaleBounceStartScale, duration)
                    .setEase(LeanTweenType.easeInBack)
                    .setIgnoreTimeScale(unscaled)
                    .setOnComplete(OnCloseComplete);
            }
            else if (style == AnimationStyle.FadeOnly)
            {
                LeanTween.value(gameObject, 0f, 1f, duration)
                    .setIgnoreTimeScale(unscaled)
                    .setOnComplete(OnCloseComplete);
            }
            else
            {
                Vector2 hidden = boxOriginalAnchoredPos + GetHiddenOffset();
                LeanTween.move(box, hidden, duration)
                    .setEase(LeanTweenType.easeInBack)
                    .setIgnoreTimeScale(unscaled)
                    .setOnComplete(OnCloseComplete);
            }
        }
        else
        {
            // FIX BUG PAUSE: sama seperti di PlayOpen(), ganti Invoke() -> coroutine unscaled-aware.
            fallbackRoutine = StartCoroutine(DelayedCallback(OnCloseComplete, duration, unscaled));
        }
    }

    private void OnCloseComplete()
    {
        isAnimating = false;
        onClosed?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator DelayedCallback(System.Action callback, float delay, bool unscaled)
    {
        if (unscaled)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return new WaitForSeconds(delay);

        fallbackRoutine = null;
        callback?.Invoke();
    }

    private void CancelFallback()
    {
        if (fallbackRoutine != null)
        {
            StopCoroutine(fallbackRoutine);
            fallbackRoutine = null;
        }
    }

    private void OnDisable()
    {
        // Jaga-jaga: batalkan semua tween yang menempel biar tidak ada error
        // "tween masih jalan padahal objek sudah nonaktif" saat scene berpindah cepat.
        if (box != null) LeanTween.cancel(box.gameObject);
        if (background != null) LeanTween.cancel(background.gameObject);
        CancelFallback();
        isAnimating = false;
    }

    public bool IsAnimating => isAnimating;
}