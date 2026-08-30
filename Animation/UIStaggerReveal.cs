using UnityEngine;

// UIStaggerReveal.cs
// Script UMUM (reusable) untuk animasi "muncul bergiliran satu-satu" pada sekumpulan
// item UI, misalnya kartu karakter di Character Selection, thumbnail Map Selection,
// atau daftar opsi Settings. Efek ini yang bikin panel grid terasa hidup, bukan
// muncul mendadak/patah-patah.
//
// CARA ASSIGN DI UNITY:
// 1. Pilih GameObject "Panel" atau "Grid/Container" yang isinya banyak child item
//    (misal: Panel Character Selection yang di dalamnya ada kartu-kartu karakter).
// 2. Add Component -> UIStaggerReveal DI GAMEOBJECT PARENT/CONTAINER-nya
//    (bukan di masing-masing item, cukup satu script untuk semua anaknya).
// 3. Biarkan field "Items" kosong -> otomatis akan animasikan SEMUA child langsung
//    di bawahnya. Atau drag manual RectTransform tertentu ke list "Items" kalau
//    hanya sebagian yang mau dianimasikan.
// 4. Atur "Stagger Interval" (jeda antar item, default 0.06 detik) dan "Duration".
// 5. Script otomatis jalan tiap kali GameObject Panel ini di-SetActive(true) / OnEnable.
//
// ============================== FIX BUG: ANIMASI MACET SAAT SCENE MAP DI-PAUSE ==============================
// Sama seperti UIPanelAnimator: kalau item ini muncul di dalam Panel yang dibuka SAAT
// Time.timeScale = 0 (misal daftar opsi di dalam Pause Menu di scene Map), animasi stagger
// akan macet total karena LeanTween secara default memakai scaled delta time (Time.deltaTime),
// yang bernilai 0 saat game di-pause. Sebelumnya field "useUnscaledTime" harus dicentang
// manual di Inspector supaya tetap jalan.
//
// SEKARANG: ditambahkan deteksi OTOMATIS lewat "EffectiveUnscaledTime" — kalau Time.timeScale
// terdeteksi 0 saat OnEnable() dipanggil, animasi otomatis pakai unscaled time juga TANPA
// perlu centang apa pun. Kalau game tidak sedang pause, perilaku sama seperti sebelumnya.
// ================================================================================================================
public class UIStaggerReveal : MonoBehaviour
{
    public enum RevealStyle
    {
        FadeSlideUp,
        FadeScale
    }

    [Header("Items (kosongkan untuk pakai semua child otomatis)")]
    [SerializeField] private RectTransform[] items;

    [Header("Timing")]
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float staggerInterval = 0.06f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private RevealStyle style = RevealStyle.FadeSlideUp;
    [SerializeField] private float slideOffset = 40f;
    [Tooltip("Paksa unscaled time selalu aktif. TIDAK WAJIB dicentang lagi — kalau Time.timeScale " +
             "terdeteksi 0 (game sedang pause) saat item ini muncul, unscaled time otomatis dipakai " +
             "walau field ini dibiarkan OFF.")]
    [SerializeField] private bool useUnscaledTime = false;

    private Vector2[] originalPositions;
    private Vector3[] originalScales;
    private CanvasGroup[] canvasGroups;
    private bool cached = false;

    // FIX BUG PAUSE: lihat penjelasan lengkap di komentar besar bagian atas file.
    private bool EffectiveUnscaledTime => useUnscaledTime || Time.timeScale <= 0f;

    private void OnEnable()
    {
        CacheItems();
        PlayStagger();
    }

    private void CacheItems()
    {
        if (cached) return;

        if (items == null || items.Length == 0)
        {
            int childCount = transform.childCount;
            items = new RectTransform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                items[i] = transform.GetChild(i) as RectTransform;
            }
        }

        int n = items.Length;
        originalPositions = new Vector2[n];
        originalScales = new Vector3[n];
        canvasGroups = new CanvasGroup[n];

        for (int i = 0; i < n; i++)
        {
            if (items[i] == null) continue;

            originalPositions[i] = items[i].anchoredPosition;
            originalScales[i] = items[i].localScale;

            CanvasGroup cg = items[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = items[i].gameObject.AddComponent<CanvasGroup>();
            canvasGroups[i] = cg;
        }

        cached = true;
    }

    private void PlayStagger()
    {
        if (items == null) return;

        bool unscaled = EffectiveUnscaledTime;

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform item = items[i];
            if (item == null) continue;

            LeanTween.cancel(item.gameObject);

            CanvasGroup cg = canvasGroups[i];
            float delay = startDelay + (i * staggerInterval);

            if (cg != null)
            {
                cg.alpha = 0f;
                LeanTween.alphaCanvas(cg, 1f, duration)
                    .setEase(LeanTweenType.easeOutSine)
                    .setIgnoreTimeScale(unscaled)
                    .setDelay(delay);
            }

            if (style == RevealStyle.FadeSlideUp)
            {
                item.anchoredPosition = originalPositions[i] + new Vector2(0f, -slideOffset);
                item.localScale = originalScales[i];

                LeanTween.move(item, originalPositions[i], duration)
                    .setEase(LeanTweenType.easeOutBack)
                    .setIgnoreTimeScale(unscaled)
                    .setDelay(delay);
            }
            else // FadeScale
            {
                item.anchoredPosition = originalPositions[i];
                item.localScale = originalScales[i] * 0.8f;

                LeanTween.scale(item, originalScales[i], duration)
                    .setEase(LeanTweenType.easeOutBack)
                    .setIgnoreTimeScale(unscaled)
                    .setDelay(delay);
            }
        }
    }

    private void OnDisable()
    {
        if (items == null) return;
        foreach (var item in items)
        {
            if (item != null) LeanTween.cancel(item.gameObject);
        }
    }
}