using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DialogBox.cs — Versi upgrade
// Field "box" dan "background" SENGAJA tidak diubah namanya,
// supaya referensi yang sudah kamu assign di Inspector TIDAK hilang / tidak perlu di-assign ulang.
//
// ============================== FIX BUG: ANIMASI MACET SAAT SCENE MAP DI-PAUSE ==============================
// Sama seperti UIPanelAnimator/UIStaggerReveal: kalau dialog ini muncul SAAT Time.timeScale = 0
// (misal dialog konfirmasi yang dipanggil dari dalam Pause Menu di scene Map), animasinya akan
// macet total karena LeanTween secara default memakai scaled delta time, yang bernilai 0 saat
// game di-pause. Field "useUnscaledTime" tetap ada, tapi sekarang TIDAK WAJIB dicentang manual —
// ditambahkan deteksi OTOMATIS lewat "EffectiveUnscaledTime": kalau Time.timeScale terdeteksi 0
// saat dialog dibuka/ditutup, unscaled time otomatis dipakai. Fallback "Invoke()" yang dipakai
// waktu "box" tidak di-assign juga diganti jadi coroutine + WaitForSecondsRealtime, karena
// Invoke() ikut mengikuti Time.timeScale dan bisa "menggantung" selamanya saat game di-pause.
// ================================================================================================================
public class DialogBox : MonoBehaviour
{
    public Transform box;
    public CanvasGroup background;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float moveDuration = 0.45f;
    [SerializeField] private float openDelay = 0.08f;
    [Tooltip("Paksa unscaled time selalu aktif. TIDAK WAJIB dicentang lagi — kalau Time.timeScale " +
             "terdeteksi 0 (game sedang pause) saat dialog ini muncul, unscaled time otomatis dipakai " +
             "walau field ini dibiarkan OFF.")]
    [SerializeField] private bool useUnscaledTime = false; // aktifkan jika dialog ini SELALU muncul saat Time.timeScale = 0

    private Vector3 boxOriginalLocalPos;
    private bool cachedOriginalPos = false;
    private Coroutine fallbackRoutine;

    // FIX BUG PAUSE: lihat penjelasan lengkap di komentar besar bagian atas file.
    private bool EffectiveUnscaledTime => useUnscaledTime || Time.timeScale <= 0f;

    private void Awake()
    {
        CacheOriginalPos();
    }

    private void CacheOriginalPos()
    {
        if (!cachedOriginalPos && box != null)
        {
            boxOriginalLocalPos = box.localPosition;
            cachedOriginalPos = true;
        }
    }

    private void OnEnable()
    {
        CacheOriginalPos();
        bool unscaled = EffectiveUnscaledTime;
        CancelFallback();

        // Hentikan tween sebelumnya biar tidak tabrakan / macet saat panel dibuka-tutup cepat
        if (background != null) LeanTween.cancel(background.gameObject);
        if (box != null) LeanTween.cancel(box.gameObject);

        if (background != null)
        {
            background.alpha = 0f;
            background.LeanAlpha(1f, fadeDuration)
                .setEase(LeanTweenType.easeOutSine)
                .setIgnoreTimeScale(unscaled);
        }

        if (box != null)
        {
            box.localPosition = new Vector3(boxOriginalLocalPos.x, boxOriginalLocalPos.y - Screen.height, boxOriginalLocalPos.z);
            box.localScale = Vector3.one * 0.92f;

            box.LeanMoveLocalY(boxOriginalLocalPos.y, moveDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setIgnoreTimeScale(unscaled)
                .delay = openDelay;

            box.LeanScale(Vector3.one, moveDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setIgnoreTimeScale(unscaled)
                .delay = openDelay;
        }
    }

    public void CloseDialog()
    {
        CacheOriginalPos();
        bool unscaled = EffectiveUnscaledTime;
        CancelFallback();

        if (background != null) LeanTween.cancel(background.gameObject);
        if (box != null) LeanTween.cancel(box.gameObject);

        if (background != null)
        {
            background.LeanAlpha(0f, fadeDuration)
                .setEase(LeanTweenType.easeInSine)
                .setIgnoreTimeScale(unscaled);
        }

        if (box != null)
        {
            box.LeanMoveLocalY(boxOriginalLocalPos.y - Screen.height, moveDuration)
                .setEase(LeanTweenType.easeInBack)
                .setIgnoreTimeScale(unscaled)
                .setOnComplete(OnComplete);

            box.LeanScale(Vector3.one * 0.92f, moveDuration)
                .setEase(LeanTweenType.easeInBack)
                .setIgnoreTimeScale(unscaled);
        }
        else
        {
            // FIX BUG PAUSE: dulu pakai Invoke(), yang ikut mengikuti Time.timeScale (jadi
            // ikut "menggantung" saat pause). Sekarang pakai coroutine + WaitForSecondsRealtime
            // saat unscaled aktif, supaya panel tetap tertutup walau game sedang di-pause.
            fallbackRoutine = StartCoroutine(DelayedCallback(OnComplete, fadeDuration, unscaled));
        }
    }

    private void OnComplete()
    {
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
        CancelFallback();
    }
}